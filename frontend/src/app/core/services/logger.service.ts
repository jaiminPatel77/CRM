import { Injectable, inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { HttpClient } from '@angular/common/http';

export enum LogLevel {
    Debug = 0,
    Info = 1,
    Warn = 2,
    Error = 3,
    Off = 4
}

@Injectable({
    providedIn: 'root'
})
export class LoggerService {

    private _level: LogLevel = LogLevel.Info;
    private _http = inject(HttpClient);
    private _remoteLogUrl = environment.Setting.remoteLogUrl;

    constructor() {
        if (!environment.production) {
            this._level = LogLevel.Debug;
        }
    }

    debug(msg: string, ...optionalParams: any[]) {
        this.log(LogLevel.Debug, msg, optionalParams);
    }

    info(msg: string, ...optionalParams: any[]) {
        this.log(LogLevel.Info, msg, optionalParams);
    }

    warn(msg: string, ...optionalParams: any[]) {
        this.log(LogLevel.Warn, msg, optionalParams);
    }

    error(msg: string, ...optionalParams: any[]) {
        this.log(LogLevel.Error, msg, optionalParams);
    }

    private log(level: LogLevel, msg: string, optionalParams: any[]) {
        if (level < this._level) {
            return;
        }

        const timestamp = new Date().toISOString();
        const formattedMsg = `[${timestamp}] [${LogLevel[level]}] ${msg}`;

        switch (level) {
            case LogLevel.Debug:
                console.debug(`%c${formattedMsg}`, 'color: #3498db', ...optionalParams);
                break;
            case LogLevel.Info:
                console.info(`%c${formattedMsg}`, 'color: #2ecc71', ...optionalParams);
                break;
            case LogLevel.Warn:
                console.warn(`%c${formattedMsg}`, 'color: #f39c12', ...optionalParams);
                break;
            case LogLevel.Error:
                console.error(`%c${formattedMsg}`, 'color: #e74c3c', ...optionalParams);
                break;
        }

        // Remote Logging for Errors
        if (level >= LogLevel.Error && this._remoteLogUrl) {
            this.sendRemoteLog(level, msg, timestamp, optionalParams);
        }
    }

    private sendRemoteLog(level: LogLevel, msg: string, timestamp: string, optionalParams: any[]) {
        const logEntry = {
            level: LogLevel[level],
            message: msg,
            timestamp: timestamp,
            additionalDetails: optionalParams
        };

        this._http.post(this._remoteLogUrl!, logEntry).subscribe({
            next: () => {
                // Log sent successfully (silently ignore) 
            },
            error: (err) => {
                console.error('Failed to send remote log', err);
            }
        });
    }
}
