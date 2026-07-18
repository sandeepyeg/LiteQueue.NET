import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService, DeadLetterMessage } from '../services/api.service';

@Component({
  selector: 'app-deadletter',
  template: `
    <div class="back-row"><a routerLink="/queues/{{ queueName }}">&larr; Back to Queue</a></div>

    <h1>Dead Letters: {{ queueName }}</h1>

    <div *ngIf="error" class="error">{{ error }}</div>

    <div class="dl-list" *ngIf="messages.length; else noMessages">
      <div class="dl-card" *ngFor="let dl of messages">
        <div class="dl-header">
          <code class="dl-id">{{ dl.id }}</code>
          <span class="dl-attempts">{{ dl.deliveryAttempts }} attempts</span>
        </div>
        <p class="dl-body" *ngIf="dl.body">{{ dl.body | slice:0:300 }}{{ dl.body.length > 300 ? '...' : '' }}</p>
        <div class="dl-meta">
          <span *ngIf="dl.errorMessage" class="dl-error">{{ dl.errorMessage }}</span>
          <span *ngIf="dl.failureReason" class="dl-reason">{{ dl.failureReason }}</span>
          <span class="dl-date">{{ dl.deadLetteredAt | date:'medium' }}</span>
        </div>
        <div class="dl-actions">
          <button class="btn-redrive" (click)="redrive(dl.id)">Redrive</button>
          <button class="btn-delete" (click)="deleteMessage(dl.id)">Delete</button>
        </div>
      </div>
    </div>
    <ng-template #noMessages><p class="empty">No dead letter messages for this queue.</p></ng-template>
  `,
  styles: [`
    .back-row { margin-bottom: 16px; }
    .back-row a { color: #3b82f6; text-decoration: none; font-size: 14px; }
    h1 { color: #1a1a2e; margin-bottom: 24px; }
    .error { background: #fee2e2; color: #991b1b; padding: 12px; border-radius: 6px; margin-bottom: 16px; }
    .dl-list { display: flex; flex-direction: column; gap: 12px; }
    .dl-card { background: #fff; border-radius: 8px; padding: 16px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); }
    .dl-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .dl-id { font-size: 12px; background: #f3f4f6; padding: 2px 6px; border-radius: 4px; }
    .dl-attempts { font-size: 12px; color: #f59e0b; font-weight: 600; }
    .dl-body { font-size: 13px; color: #4b5563; margin: 8px 0; word-break: break-all; }
    .dl-meta { display: flex; flex-wrap: wrap; gap: 8px; font-size: 12px; color: #9ca3af; margin-top: 8px; }
    .dl-error { color: #e94560; }
    .dl-reason { color: #f59e0b; }
    .dl-date { margin-left: auto; }
    .dl-actions { display: flex; gap: 8px; margin-top: 12px; }
    .btn-redrive { padding: 6px 14px; background: #3b82f6; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 12px; }
    .btn-redrive:hover { background: #2563eb; }
    .btn-delete { padding: 6px 14px; background: #e94560; color: #fff; border: none; border-radius: 4px; cursor: pointer; font-size: 12px; }
    .btn-delete:hover { background: #dc2626; }
    .empty { color: #9ca3af; text-align: center; padding: 32px; }
  `]
})
export class DeadLetterComponent implements OnInit {
  queueName = '';
  messages: DeadLetterMessage[] = [];
  error = '';

  constructor(private route: ActivatedRoute, private api: ApiService) { }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.queueName = params['queueName'];
      this.load();
    });
  }

  load(): void {
    this.error = '';
    this.api.getDeadLetters(this.queueName).subscribe({
      next: m => this.messages = m,
      error: () => this.error = 'Failed to load dead letter messages.'
    });
  }

  redrive(messageId: string): void {
    this.api.redriveDeadLetter(this.queueName, messageId).subscribe({
      next: () => this.load(),
      error: () => this.error = 'Failed to redrive message.'
    });
  }

  deleteMessage(messageId: string): void {
    this.api.deleteDeadLetter(this.queueName, messageId).subscribe({
      next: () => this.load(),
      error: () => this.error = 'Failed to delete message.'
    });
  }
}
