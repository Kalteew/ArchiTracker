/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
 
import type { ArchipelagoRoomRequest } from '../models/ArchipelagoRoomRequest';
import type { ArchipelagoRoomResponse } from '../models/ArchipelagoRoomResponse';
import type { ArchipelagoRoomVisualizationResponse } from '../models/ArchipelagoRoomVisualizationResponse';
import type { CancelablePromise } from '../core/CancelablePromise';
import type { BaseHttpRequest } from '../core/BaseHttpRequest';
export class ArchipelagoRoomService {
    constructor(public readonly httpRequest: BaseHttpRequest) {}
    /**
     * @param requestBody
     * @returns ArchipelagoRoomResponse OK
     * @throws ApiError
     */
    public postApiArchipelagoRoom(
        requestBody: ArchipelagoRoomRequest,
    ): CancelablePromise<ArchipelagoRoomResponse> {
        return this.httpRequest.request({
            method: 'POST',
            url: '/api/archipelago-room',
            body: requestBody,
            mediaType: 'application/json',
            errors: {
                400: `Bad Request`,
            },
        });
    }
    /**
     * @param roomCode
     * @returns ArchipelagoRoomVisualizationResponse OK
     * @throws ApiError
     */
    public getApiArchipelagoRoom(
        roomCode: string,
    ): CancelablePromise<ArchipelagoRoomVisualizationResponse> {
        return this.httpRequest.request({
            method: 'GET',
            url: '/api/archipelago-room/{roomCode}',
            path: {
                'roomCode': roomCode,
            },
            errors: {
                404: `Not Found`,
            },
        });
    }
}
