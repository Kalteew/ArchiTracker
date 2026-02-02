import { fileURLToPath } from "node:url"
import { dirname, join } from "node:path"
import { execSync, spawn } from "node:child_process"
import { writeFileSync } from "node:fs"
import { setTimeout as sleep } from "node:timers/promises"
import { generate } from "openapi-typescript-codegen"

const __dirname = dirname(fileURLToPath(import.meta.url))
const frontendRoot = join(__dirname, "..")
const backendRoot = join(frontendRoot, "..", "ArchiTrackerBE")
const configuration = process.env.API_BUILD_CONFIGURATION ?? "Release"
const baseUrl = process.env.API_SERVER_URL ?? "http://localhost:5099"
const swaggerDoc = process.env.API_SWAGGER_DOC ?? "v1"
const specPath = join(backendRoot, "openapi.json")

const run = (cmd) => {
  console.log(`\n$ ${cmd}`)
  execSync(cmd, {
    cwd: backendRoot,
    stdio: "inherit",
    env: process.env,
  })
}

const fetchOpenApi = async () => {
  const url = `${baseUrl}/openapi/${swaggerDoc}.json`
  for (let attempt = 0; attempt < 20; attempt += 1) {
    try {
      const response = await fetch(url)
      if (response.ok) {
        const json = await response.text()
        writeFileSync(specPath, json)
        return
      }
    } catch {
      // swallow, server probably not ready yet
    }
    await sleep(500)
  }

  throw new Error(`Could not download OpenAPI document from ${url}`)
}

run("dotnet build ArchiTrackerBE.csproj --configuration " + configuration)

console.log("\nStarting backend to snapshot OpenAPI...")
const apiProcess = spawn("dotnet", ["run", "--no-build", "--configuration", configuration, "--urls", baseUrl], {
  cwd: backendRoot,
  stdio: "inherit",
  env: { ...process.env, ASPNETCORE_ENVIRONMENT: "Development" },
})

const stopServer = () =>
  new Promise((resolve) => {
    apiProcess.once("exit", resolve)
    apiProcess.kill("SIGINT")
  })

try {
  await fetchOpenApi()
} finally {
  await stopServer()
}

await generate({
  input: specPath,
  output: join(frontendRoot, "src", "api"),
  httpClient: "fetch",
  useUnionTypes: true,
  exportModels: true,
  exportServices: true,
  exportCore: true,
  clientName: "ApiClient",
})

console.log("\n✅ API client generated at src/api")
