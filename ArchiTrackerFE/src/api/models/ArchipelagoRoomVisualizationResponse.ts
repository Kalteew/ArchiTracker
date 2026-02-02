/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
 
import type { RoomStatusDto } from './RoomStatusDto';
import type { TrackerHintDto } from './TrackerHintDto';
import type { TrackerPlayerDto } from './TrackerPlayerDto';
export type ArchipelagoRoomVisualizationResponse = {
    roomCode?: string;
    trackerUrl?: string;
    roomStatus?: (null | RoomStatusDto);
    players?: Array<TrackerPlayerDto>;
    hints?: Array<TrackerHintDto>;
};

