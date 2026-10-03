import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, Router, RouterStateSnapshot } from '@angular/router';
import { map, mergeMap, of, catchError } from 'rxjs';
import { AuthService } from '../../features/auth/services/auth.service';
import { UserService } from '../../features/auth/services/user.service';

export const authGuard: CanActivateFn = (route, state) => {
  const _authService = inject(AuthService);
  const _userService = inject(UserService);
  const _router = inject(Router);
  return checkLogin(route, state, _authService, _userService, _router);
}

function checkLogin(route: ActivatedRouteSnapshot, state: RouterStateSnapshot, _authService: AuthService, _userService: UserService, _router: Router) {
  return _authService.isAuthenticated()
    .pipe(
      map((data) => data),
      mergeMap((isValid) => { //IsAuthenticated response.
        if (!isValid) {
          return of(undefined);
        } else {
          let userId = _authService?.getCurrentToken?.userId();
          if (_userService.currentUser.id == userId) {
            return of(_userService.currentUser);
          } else {
            return _userService?.loadCurrentUser(userId);
          }
        }
      }),
      mergeMap((user) => { //Current user loading..
        setIssueUrl(state, _authService);
        if (!user) {
          //UserId not found so return to login page!
          _authService.redirectUrl = state.url;
          _router.navigate(['/auth']);
          return of(false);
        } else {
          //other lic loading here!
          if (route.data && route.data['permission']) {
            let isAllowed = _userService.permissionService.isAllowed(route.data['permissionFor'], route.data['permission']);
            if (isAllowed) {
              return of(true);
            } else {
              _authService.redirectUrl = state.url;
              _router.navigate(['/auth']);
              return of(false);
            }
          }
          return of(true)
        }
      }),
      catchError((error) => {
        console.dir(error);
        _authService.redirectUrl = state.url;
        _router.navigate(['/auth']);
        return of(false);
      })
    );
};

function setIssueUrl(state: RouterStateSnapshot, _authService: AuthService) {
  if (state.url.includes('issue/issues')) {
    _authService.issueUrl = state.url;
  }
}



