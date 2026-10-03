import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

@Component({
    selector: 'app-forgot-password-confirm',
    imports: [RouterLink, TranslateModule],
    templateUrl: './forgot-password-confirm.component.html',
    styleUrl: './forgot-password-confirm.component.scss'
})
export class ForgotPasswordConfirmComponent implements OnInit, OnDestroy {

  constructor(
    protected _router: Router
  ) { }

  ngOnInit() {

  }

  ngOnDestroy() {

  }

}
