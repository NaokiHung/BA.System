import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Angular 19 獨立根組件
 * 修正：移除對 LayoutComponent 的直接引用，改用路由
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet></router-outlet>',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'budget-assistant-web';
}