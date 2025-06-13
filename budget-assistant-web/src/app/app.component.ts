/**
 * 檔案路徑: budget-assistant-web/src/app/app.component.ts
 * 修正的根組件，移除除錯資訊
 */

import { Component } from '@angular/core';
import { LayoutComponent } from './layout/layout.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [LayoutComponent],
  template: '<app-layout></app-layout>',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'budget-assistant-web';
}