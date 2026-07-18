import { Component, OnInit } from '@angular/core';
import { ApiService, QueueSummary, AllStatistics } from '../services/api.service';

@Component({
  selector: 'app-dashboard',
  template: `
    <h1>Dashboard</h1>

    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-value">{{ aggStats.totalQueues }}</div>
        <div class="stat-label">Total Queues</div>
      </div>
      <div class="stat-card ready">
        <div class="stat-value">{{ aggStats.totalReadyMessages }}</div>
        <div class="stat-label">Ready Messages</div>
      </div>
      <div class="stat-card inflight">
        <div class="stat-value">{{ aggStats.totalInFlightMessages }}</div>
        <div class="stat-label">In-Flight</div>
      </div>
      <div class="stat-card delayed">
        <div class="stat-value">{{ aggStats.totalDelayedMessages }}</div>
        <div class="stat-label">Delayed</div>
      </div>
      <div class="stat-card dead">
        <div class="stat-value">{{ aggStats.totalDeadLetterMessages }}</div>
        <div class="stat-label">Dead Letters</div>
      </div>
    </div>

    <h2>Processing Overview</h2>
    <div class="chart-bar">
      <div class="bar-segment ready-seg" [style.flex]="aggStats.totalReadyMessages || 0.01">
        {{ aggStats.totalReadyMessages }} Ready
      </div>
      <div class="bar-segment inflight-seg" [style.flex]="aggStats.totalInFlightMessages || 0.01">
        {{ aggStats.totalInFlightMessages }} In-Flight
      </div>
      <div class="bar-segment delayed-seg" [style.flex]="aggStats.totalDelayedMessages || 0.01">
        {{ aggStats.totalDelayedMessages }} Delayed
      </div>
      <div class="bar-segment dead-seg" [style.flex]="aggStats.totalDeadLetterMessages || 0.01">
        {{ aggStats.totalDeadLetterMessages }} DLQ
      </div>
    </div>

    <h2>Queues</h2>
    <div class="queue-table" *ngIf="queues.length; else noQueues">
      <div class="table-header">
        <span class="col-name">Name</span>
        <span class="col-status">Status</span>
        <span class="col-num">Ready</span>
        <span class="col-num">In-Flight</span>
        <span class="col-num">Delayed</span>
        <span class="col-num">DLQ</span>
      </div>
      <a class="table-row" *ngFor="let q of queues" [routerLink]="['/queues', q.name]">
        <span class="col-name">{{ q.name }}</span>
        <span class="col-status">
          <span class="badge" [class.badge-active]="q.status === 'Active'" [class.badge-paused]="q.status === 'Paused'">{{ q.status }}</span>
        </span>
        <span class="col-num">{{ q.readyCount }}</span>
        <span class="col-num">{{ q.inFlightCount }}</span>
        <span class="col-num">{{ q.delayedCount }}</span>
        <span class="col-num">{{ q.deadLetterCount }}</span>
      </a>
    </div>
    <ng-template #noQueues>
      <p class="empty">No queues found.</p>
    </ng-template>
  `,
  styles: [`
    h1 { color: #1a1a2e; margin-bottom: 24px; }
    h2 { color: #1a1a2e; margin: 32px 0 16px; }
    .stats-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 16px; }
    .stat-card {
      background: #fff; border-radius: 8px; padding: 20px;
      box-shadow: 0 1px 4px rgba(0,0,0,0.08);
      border-left: 4px solid #16213e;
    }
    .stat-card.ready { border-left-color: #4ecca3; }
    .stat-card.inflight { border-left-color: #3b82f6; }
    .stat-card.delayed { border-left-color: #f59e0b; }
    .stat-card.dead { border-left-color: #e94560; }
    .stat-value { font-size: 32px; font-weight: 700; color: #1a1a2e; }
    .stat-label { font-size: 13px; color: #6b7280; margin-top: 4px; text-transform: uppercase; letter-spacing: 0.5px; }
    .chart-bar {
      display: flex; height: 36px; border-radius: 6px; overflow: hidden;
      font-size: 12px; font-weight: 600;
    }
    .bar-segment { display: flex; align-items: center; justify-content: center; color: #fff; min-width: 0; }
    .ready-seg { background: #4ecca3; }
    .inflight-seg { background: #3b82f6; }
    .delayed-seg { background: #f59e0b; }
    .dead-seg { background: #e94560; }
    .queue-table { background: #fff; border-radius: 8px; box-shadow: 0 1px 4px rgba(0,0,0,0.08); overflow: hidden; }
    .table-header, .table-row {
      display: grid; grid-template-columns: 2fr 1fr 1fr 1fr 1fr 1fr;
      padding: 12px 20px; align-items: center; font-size: 14px;
    }
    .table-header { background: #f8f9fa; font-weight: 600; color: #6b7280; text-transform: uppercase; font-size: 12px; letter-spacing: 0.5px; }
    .table-row { color: #1a1a2e; text-decoration: none; border-top: 1px solid #f0f0f0; transition: background 0.15s; }
    .table-row:hover { background: #f8f9fa; }
    .col-name { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .col-num { text-align: right; font-variant-numeric: tabular-nums; }
    .badge { display: inline-block; padding: 2px 8px; border-radius: 10px; font-size: 11px; font-weight: 600; }
    .badge-active { background: #d1fae5; color: #065f46; }
    .badge-paused { background: #fef3c7; color: #92400e; }
    .empty { color: #9ca3af; text-align: center; padding: 32px; }
  `]
})
export class DashboardComponent implements OnInit {
  queues: QueueSummary[] = [];
  aggStats: AllStatistics = { totalQueues: 0, totalReadyMessages: 0, totalInFlightMessages: 0, totalDelayedMessages: 0, totalDeadLetterMessages: 0 };

  constructor(private api: ApiService) { }

  ngOnInit(): void {
    this.api.getStatistics().subscribe(data => {
      this.queues = data;
      this.aggStats = {
        totalQueues: data.length,
        totalReadyMessages: data.reduce((sum, q) => sum + q.readyCount, 0),
        totalInFlightMessages: data.reduce((sum, q) => sum + q.inFlightCount, 0),
        totalDelayedMessages: data.reduce((sum, q) => sum + q.delayedCount, 0),
        totalDeadLetterMessages: data.reduce((sum, q) => sum + q.deadLetterCount, 0),
      };
    });
  }
}
