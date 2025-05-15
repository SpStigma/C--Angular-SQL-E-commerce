export interface User {
  id: number;
  email: string;
  status: 'user' | 'admin';
}
