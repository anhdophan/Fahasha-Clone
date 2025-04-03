using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAn.Models
{
    //Tạo lớp cartItem chứa các dòng sản phẩm trong giỏ hàng
    public class CartItem
    {
        public Product _product { get; set; }
        public int _quantity { get; set; }
    }
    public class Cart
    {
        List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }
        public void Add_Product_Cart(Product _pro, int _quan = 1)
        {
            var item = Items.FirstOrDefault(s => s._product.ProductID == _pro.ProductID);
            if (item == null) //nếu giỏ hàng rỗng thì thêm dòng hàng mới vào giỏ
                items.Add(new CartItem
                {
                    _product = _pro,
                    _quantity = _quan
                });
            else
                item._quantity += _quan;   //tổng số lượng trong giỏ hàng được cộng dồn
        }
        public int Total_quantity()
        {
            return items.Sum(s => s._quantity);
        }
        public decimal Total_money()
        {
            var total = items.Sum(s => s._quantity * s._product.Price);
            return (decimal)total;
        }
        public void Update_quantity(int id, int _new_quan)
        {
            var item = items.Find(s => s._product.ProductID == id);
            if (item != null)
            {
                if (items.Find(s => s._product.RemainQuantity < _new_quan) != null || _new_quan < 0)
                    item._quantity = 1;
                else
                    item._quantity = _new_quan;
            }

        }
        public void Remove_CartItem(int id)
        {
            items.RemoveAll(s => s._product.ProductID == id);
        }
        public void ClearCart()
        {
            items.Clear();
        }
    }
}