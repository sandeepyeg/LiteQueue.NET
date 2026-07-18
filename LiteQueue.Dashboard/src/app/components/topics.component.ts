import { Component, OnInit } from '@angular/core';
import { ApiService, TopicInfo, TopicSubscriptionInfo } from '../services/api.service';

@Component({
  selector: 'app-topics',
  template: `
    <h1>Topics</h1>

    <div *ngIf="error" class="error">{{ error }}</div>

    <div *ngIf="topics.length; else noTopics">
      <div class="topic-card" *ngFor="let t of topics" (click)="toggle(t.name)">
        <div class="topic-header">
          <span class="topic-name">{{ t.name }}</span>
          <span class="topic-count">{{ t.subscriptionCount }} subscriptions</span>
        </div>
        <div class="subs-list" *ngIf="expandedTopic === t.name">
          <div *ngIf="loadingSubs" class="loading">Loading subscriptions...</div>
          <div class="sub-item" *ngFor="let s of subscriptions">
            <span>{{ s.subscriptionName }}</span>
            <span class="sub-topic" *ngIf="s.topicName">topic: {{ s.topicName }}</span>
          </div>
          <p *ngIf="!loadingSubs && !subscriptions.length" class="empty-sub">No subscriptions.</p>
        </div>
      </div>
    </div>
    <ng-template #noTopics><p class="empty">No topics found.</p></ng-template>
  `,
  styles: [`
    h1 { color: #1a1a2e; margin-bottom: 24px; }
    .error { background: #fee2e2; color: #991b1b; padding: 12px; border-radius: 6px; margin-bottom: 16px; }
    .topic-card {
      background: #fff; border-radius: 8px; padding: 16px 20px;
      box-shadow: 0 1px 4px rgba(0,0,0,0.08); margin-bottom: 12px; cursor: pointer;
      transition: box-shadow 0.15s;
    }
    .topic-card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.12); }
    .topic-header { display: flex; justify-content: space-between; align-items: center; }
    .topic-name { font-size: 16px; font-weight: 600; color: #1a1a2e; }
    .topic-count { font-size: 13px; color: #6b7280; }
    .subs-list { margin-top: 12px; border-top: 1px solid #f0f0f0; padding-top: 12px; }
    .sub-item {
      display: flex; justify-content: space-between; padding: 6px 0;
      font-size: 14px; color: #374151;
    }
    .sub-topic { font-size: 12px; color: #9ca3af; }
    .loading { font-size: 13px; color: #9ca3af; padding: 8px 0; }
    .empty, .empty-sub { color: #9ca3af; text-align: center; padding: 16px; font-size: 13px; }
  `]
})
export class TopicsComponent implements OnInit {
  topics: TopicInfo[] = [];
  subscriptions: TopicSubscriptionInfo[] = [];
  expandedTopic: string | null = null;
  loadingSubs = false;
  error = '';

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    this.api.listTopics().subscribe({
      next: t => this.topics = t,
      error: () => this.error = 'Failed to load topics.'
    });
  }

  toggle(topicName: string): void {
    if (this.expandedTopic === topicName) {
      this.expandedTopic = null;
      this.subscriptions = [];
      return;
    }
    this.expandedTopic = topicName;
    this.loadingSubs = true;
    this.subscriptions = [];
    this.api.listSubscriptions(topicName).subscribe({
      next: s => { this.subscriptions = s; this.loadingSubs = false; },
      error: () => { this.loadingSubs = false; }
    });
  }
}
