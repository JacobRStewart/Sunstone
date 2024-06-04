export interface Cart {
    id: number
    buyerId: string
    items: CartItem[]
  }
  
  export interface CartItem {
    productId: number
    name: string
    price: number
    pictureURL: string
    brand: string
    type: string
    quantity: number
  }
  