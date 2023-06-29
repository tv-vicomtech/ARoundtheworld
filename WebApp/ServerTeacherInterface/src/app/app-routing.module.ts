import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { AuthGuard } from './auth.guard'
import { BarcharComponent } from './barchar/barchar.component'
import { DashboardComponent } from './dashboard/dashboard.component'
import { LoginOrkestraComponent } from './login-orkestra/login-orkestra.component'

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginOrkestraComponent },
  { path: 'dashboard', component: DashboardComponent, canActivate: [ AuthGuard ] },
  { path: 'estadisticas', component: BarcharComponent, canActivate: [ AuthGuard ] }
]

@NgModule({
  imports: [ RouterModule.forRoot(routes) ],
  exports: [ RouterModule ]
})
export class AppRoutingModule {}
