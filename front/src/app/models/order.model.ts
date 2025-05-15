export interface OrderItem {
  id: number;
  productId: number;
  unitPrice: number;
  quantity: number;
  product?: {
    id: number;
    name: string;
    imageUrl: string;
  };
}

export enum OrderStatus {
  Pending = 'Pending',
  Paid = 'Paid',
  Shipped = 'Shipped',
  Delivered = 'Delivered',
  Cancelled = 'Cancelled'
}

export interface Order {
  id: number;
  userId: number;
  items: OrderItem[];
  totalAmount: number;
  status: OrderStatus;
  createdAt: string;
}