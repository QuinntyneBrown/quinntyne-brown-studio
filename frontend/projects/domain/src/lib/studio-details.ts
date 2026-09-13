/** The studio's public contact details; an unset detail is null and stays off the contact page. */
export interface StudioDetails {
  id: string;
  version: number;
  email: string | null;
  phone: string | null;
  hours: string | null;
  replyNote: string | null;
}
