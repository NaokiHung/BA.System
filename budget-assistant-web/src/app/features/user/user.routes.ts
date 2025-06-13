import { Routes } from '@angular/router';
import { UserProfileComponent } from './profile/user-profile.component';

/**
 * 使用者功能路由設定
 * 檔案路徑：budget-assistant-web/src/app/features/user/user.routes.ts
 * 
 * 使用 Angular 新式路由架構 (standalone components)
 */

export const USER_ROUTES: Routes = [
  {
    path: '',
    redirectTo: 'profile',
    pathMatch: 'full'
  },
  {
    path: 'profile',
    component: UserProfileComponent,
    title: '個人資料管理'
  }
  // 未來可新增其他使用者相關頁面：
  // {
  //   path: 'preferences',
  //   component: UserPreferencesComponent,
  //   title: '個人偏好設定'
  // },
  // {
  //   path: 'security',
  //   component: UserSecurityComponent,
  //   title: '安全設定'
  // }
];