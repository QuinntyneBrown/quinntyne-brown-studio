import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Notice } from '@qbs/components';
import { SITE } from '../site.token';
import { LOGIN_SERVICE } from './login.token';
import { LoginService } from './login-service';
@Component({
  selector: 'qbs-login-page',
  imports: [FormsModule, RouterLink, Notice],
  providers: [{ provide: LOGIN_SERVICE, useClass: LoginService }],
  templateUrl: './login-page.html',
  styleUrl: './login-page.css',
})
export class LoginPage {
  readonly form = inject(LOGIN_SERVICE);
  readonly site = inject(SITE);
}
