import { Component } from '@angular/core';
import { LayoutComponent } from './layout/layout.component';

/**
 * Angular 19 獨立根組件
 * 為什麼要使用獨立元件？
 * 1. 更簡潔的架構
 * 2. 更好的 tree-shaking
 * 3. 更明確的依賴關係
 */
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