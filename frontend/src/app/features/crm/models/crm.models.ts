export enum EnumLeadStatus {
  New = 0,
  Contacted = 1,
  Qualified = 2,
  Unqualified = 3,
  Converted = 4
}

export enum EnumOpportunityStage {
  Qualification = 0,
  Proposal = 1,
  Negotiation = 2,
  ClosedWon = 3,
  ClosedLost = 4
}

export enum EnumActivityType {
  Call = 0,
  Meeting = 1,
  Email = 2,
  Task = 3,
  Note = 4
}

export interface Customer {
  id?: number;
  tenantId?: string;
  name: string;
  email?: string;
  phone?: string;
  company?: string;
  address?: string;
  industry?: string;
  notes?: string;
  createdOn?: string;
  modifiedOn?: string;
}

export interface Lead {
  id?: number;
  tenantId?: string;
  title: string;
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  company?: string;
  estimatedValue?: number;
  status: EnumLeadStatus;
  source?: string;
  assignedToUserId?: number;
  customerId?: number;
  notes?: string;
  createdOn?: string;
  modifiedOn?: string;
}

export interface Opportunity {
  id?: number;
  tenantId?: string;
  title: string;
  amount: number;
  stage: EnumOpportunityStage;
  probability: number;
  expectedCloseDate?: string;
  customerId?: number;
  leadId?: number;
  assignedToUserId?: number;
  notes?: string;
  createdOn?: string;
  modifiedOn?: string;
}

export interface Activity {
  id?: number;
  tenantId?: string;
  subject: string;
  type: EnumActivityType;
  dueDate?: string;
  isCompleted: boolean;
  description?: string;
  customerId?: number;
  leadId?: number;
  opportunityId?: number;
  assignedToUserId?: number;
  createdOn?: string;
  modifiedOn?: string;
}
