import { NgModule } from '@angular/core'
import { BrowserModule } from '@angular/platform-browser'
import { AppComponent } from './app.component'
import { AppRoutingModule } from './app-routing.module'
import { DashboardComponent } from './dashboard/dashboard.component'
import { BrowserAnimationsModule } from '@angular/platform-browser/animations'
import { MatSelectModule } from '@angular/material/select'
import { MatButtonModule } from '@angular/material/button'
import { MatCommonModule } from '@angular/material/core'
import { MatDividerModule } from '@angular/material/divider'
import { MatListModule } from '@angular/material/list'
import { MatTableModule } from '@angular/material/table'
import { BarcharComponent } from './barchar/barchar.component'
import { MatToolbarModule } from '@angular/material/toolbar'
import { MatIconModule } from '@angular/material/icon'
import { MatMenuModule } from '@angular/material/menu'
import { LoginOrkestraComponent } from './login-orkestra/login-orkestra.component'
import { FormsModule, ReactiveFormsModule } from '@angular/forms'
import { MessageService } from 'primeng/api'
import { ToastModule } from 'primeng/toast'
import { StudentService } from './services/student.service'
import { HttpClientModule } from '@angular/common/http'
import { MatInputModule } from '@angular/material/input'
import { MatFormFieldModule } from '@angular/material/form-field'
import { ScriptService } from './services/script.service'
import { HelpControlsComponent } from './help-controls/help-controls.component'
import { EarthComponent } from './earth/earth.component'

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    BarcharComponent,
    LoginOrkestraComponent,
    EarthComponent,
    HelpControlsComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatSelectModule,
    MatButtonModule,
    MatCommonModule,
    MatDividerModule,
    MatListModule,
    MatTableModule,
    MatToolbarModule,
    MatIconModule,
    MatMenuModule,
    FormsModule,
    ReactiveFormsModule,
    ToastModule,
    HttpClientModule,
    MatFormFieldModule,
    MatInputModule
  ],
  providers: [ MessageService, StudentService, ScriptService ],
  bootstrap: [ AppComponent ]
})
export class AppModule {}
