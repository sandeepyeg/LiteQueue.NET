import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule, Routes } from '@angular/router';

import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard.component';
import { QueueDetailComponent } from './components/queue-detail.component';
import { TopicsComponent } from './components/topics.component';
import { DeadLetterComponent } from './components/deadletter.component';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'queues/:name', component: QueueDetailComponent },
  { path: 'topics', component: TopicsComponent },
  { path: 'deadletters/:queueName', component: DeadLetterComponent },
  { path: '**', redirectTo: '' }
];

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    QueueDetailComponent,
    TopicsComponent,
    DeadLetterComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    RouterModule.forRoot(routes)
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
