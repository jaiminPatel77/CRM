import { Injectable, inject } from '@angular/core';
import { AppLang } from '../models/appLang';
import { LangChangeEvent, TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class AppTranslateService {

  supportedLangs: AppLang[];
  public translateService = inject(TranslateService);
  defaultLang: string;


  constructor() {
    this.supportedLangs = [new AppLang('en', 'LANG_ENGLISH'), new AppLang("fr", "LANG_FRENCH")]; //'ur', 'es', 'it', 'fa', 'de'
    this.defaultLang = 'en';
    this.translateService.addLangs(this.supportedLangs.map(e => e.key));
    this.translateService.setDefaultLang(this.defaultLang);
    const browserLang = this.translateService.getBrowserLang();
    this.translateService.use(browserLang?.match(/en|fr/) ? browserLang : this.defaultLang);

    this.translateService.onLangChange.subscribe({
      next: (event: LangChangeEvent) => {
        this.supportedLangs.forEach(langInfo => {
          langInfo.name = event.translations[langInfo.nameKey];
        });
      }
    });

  }

  changeLang(language: string) {
    this.translateService.use(language);
  }

}