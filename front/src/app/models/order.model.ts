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
  Pending   = 0,
  Paid      = 1,
  Shipped   = 2,
  Delivered = 3,
  Cancelled = 4
}

export interface Order {
  id: number;
  userId: number;
  items: OrderItem[];
  totalAmount: number;
  status: OrderStatus;
  createdAt: string;
}