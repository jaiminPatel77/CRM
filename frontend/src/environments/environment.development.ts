export class AppEnvironment {
    production: boolean = false;
    _environmentSetting?: IEnvironmentSetting;

    get Setting(): IEnvironmentSetting {
        if (!this._environmentSetting) {
            this._environmentSetting = this.getSettings();
        }
        return this._environmentSetting;
    }

    getSettings(): IEnvironmentSetting {
        const setting = {
            rootURL: '/',
            apiServiceUrl: 'http://localhost:5105',
            enableCaptcha: false,
            captchaKey: '',
            remoteLogUrl: ''
        };
        return setting;
    }
}

export interface IEnvironmentSetting {
    rootURL: string;
    apiServiceUrl: string;
    enableCaptcha: boolean;
    captchaKey: string;
    remoteLogUrl?: string;
}

export const environment = new AppEnvironment();