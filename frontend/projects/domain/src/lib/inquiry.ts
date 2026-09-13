/** A message sent from the contact page, kept for administrator review; never a booking. */
export interface Inquiry {
  id: string;
  version: number;
  reference: string;
  name: string;
  email: string;
  phone: string | null;
  interest: string;
  message: string;
  consentAt: string;
  submittedAt: string;
  state: string;
  reviewedBy: string | null;
  reviewedAt: string | null;
}
