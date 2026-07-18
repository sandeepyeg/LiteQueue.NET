import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface QueueSummary {
  name: string;
  status: string;
  readyCount: number;
  inFlightCount: number;
  delayedCount: number;
  deadLetterCount: number;
  createdAt: string;
}

export interface QueueConfiguration {
  name: string;
  status: string;
  visibilityTimeoutSeconds: number;
  messageRetentionSeconds: number;
  maxDeliveryAttempts: number;
  deadLetterRetentionSeconds: number;
  maxMessageSizeBytes: number;
  createdAt: string;
}

export interface DeadLetterMessage {
  id: string;
  originalQueueName: string;
  originalMessageId: string;
  body: string;
  deliveryAttempts: number;
  lastError?: string;
  deadLetteredAt: string;
  failureReason?: string;
  errorMessage?: string;
}

export interface TopicInfo {
  name: string;
  subscriptionCount: number;
  createdAt: string;
}

export interface TopicSubscriptionInfo {
  subscriptionName: string;
  topicName: string;
  messageFilter?: Record<string, string>;
}

export interface AllStatistics {
  totalQueues: number;
  totalReadyMessages: number;
  totalInFlightMessages: number;
  totalDelayedMessages: number;
  totalDeadLetterMessages: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) { }

  getStatistics(): Observable<QueueSummary[]> {
    return this.http.get<QueueSummary[]>(`${this.baseUrl}/admin/statistics`);
  }

  getQueueStatistics(queueName: string): Observable<QueueSummary> {
    return this.http.get<QueueSummary>(`${this.baseUrl}/queues/${queueName}/statistics`);
  }

  getQueueConfiguration(queueName: string): Observable<QueueConfiguration> {
    return this.http.get<QueueConfiguration>(`${this.baseUrl}/queues/${queueName}`);
  }

  listQueues(): Observable<QueueConfiguration[]> {
    return this.http.get<QueueConfiguration[]>(`${this.baseUrl}/queues`);
  }

  getDeadLetters(queueName: string): Observable<DeadLetterMessage[]> {
    return this.http.get<DeadLetterMessage[]>(`${this.baseUrl}/queues/${queueName}/deadletters`);
  }

  redriveDeadLetter(queueName: string, messageId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/queues/${queueName}/deadletters/${messageId}/redrive`, {});
  }

  deleteDeadLetter(queueName: string, messageId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/queues/${queueName}/deadletters/${messageId}`);
  }

  listTopics(): Observable<TopicInfo[]> {
    return this.http.get<TopicInfo[]>(`${this.baseUrl}/topics`);
  }

  getTopic(name: string): Observable<TopicInfo> {
    return this.http.get<TopicInfo>(`${this.baseUrl}/topics/${name}`);
  }

  listSubscriptions(topicName: string): Observable<TopicSubscriptionInfo[]> {
    return this.http.get<TopicSubscriptionInfo[]>(`${this.baseUrl}/topics/${topicName}/subscriptions`);
  }
}
