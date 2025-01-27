﻿using BusinessObjects;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Reflection;

namespace DataAccessObjects
{
    public class UserDAO
    {
        private readonly CageShopUni_alaContext _dbContext;
        
        public UserDAO()
        {
            _dbContext = new CageShopUni_alaContext();
        }

        public User getUserByEmail(string email)
        {
            return _dbContext.Users.FirstOrDefault(c => c.Email == email);
        }

        public string getUserImage(int Id)
        {
            var userImg = GetUserById(Id);
            return userImg?.UserImg ?? null;
        }

        public User GetUserById(int Id)
        {
            var u = _dbContext.Users.FirstOrDefault(c => c.UserId == Id);
            u.UserPassword = Utility.dycryptoString(u.UserPassword);
            return u;
        }
        public List<User> GetListUserByName(string name)
        {
            return _dbContext.Users.Where(c => c.UserName.Equals(name)).ToList();
        }

        public void Add(User User)
        {
            User.Status = "Active";
            User.UserPassword = Utility.encrytoStringKey(User.UserPassword);
            _dbContext.Add(User);
            _dbContext.SaveChanges();
        }

        public List<User> GetAll()
        {
            return _dbContext.Users.Where(u => u.RoleId!= 4).ToList();
        }

        public int Update(User User)
        {
            var user = GetUserById(User.UserId);
            if (user != null)
            {
                //cus.Email = User.Email;
                user.UserName = User.UserName;
                user.UserPassword = Utility.encrytoStringKey(User.UserPassword);
                user.Phone = User.Phone;
                user.Address = User.Address;
                user.UserImg = User.UserImg;
                user.RoleId = User.RoleId;
                user.Email = User.Email;
                user.DoB = User.DoB;
                user.Status = User.Status;
                user.Gender = User.Gender;
                return _dbContext.SaveChanges();
            }
            return 0;
        }
        public int ManagerUpdate(User User)
        {
            var user = GetUserById(User.UserId);
            if (user != null)
            {

                user.Status = User.Status;


                return _dbContext.SaveChanges();
            }
            return 0;
        }

        public void Delete(int cusID)
        {
            var flag = GetUserById(cusID);
            if (flag != null)
            {
                _dbContext.Users.Remove(flag);
                _dbContext.SaveChanges();
            }
        }
        // check User login
        public User checkUserLogin(string email, string password)
        {
            password = Utility.encrytoStringKey(password);  
            return _dbContext.Users.FirstOrDefault(c => c.Email == email && c.UserPassword == password);
        }

        public List<Role> GetRoles()
        {
            return _dbContext.Roles.Where(u => u.RoleId != 4).ToList();
        }
        public bool isEmailExisted(string email)
        {
            return _dbContext.Users.Any(e => e.Email == email);
        }

        public bool IsEmailExistedExceptEmailCurrent(string emailCheck, string emailCurrent)
        {

            return _dbContext.Users.Any(user => user.Email == emailCheck && user.Email != emailCurrent);

        }

        public int UpdateUserProfile(User User)
        {
            try
            {
                var user = GetUserById(User.UserId);
                if (user != null)
                {
                    user.Email = User.Email;
                    user.UserName = User.UserName;
                    user.Phone = User.Phone;
                    user.Address = User.Address;
                    //user.UserImg = User.UserImg;
                    user.DoB = User.DoB;
                    user.UserPassword = Utility.encrytoStringKey(User.UserPassword);
                    return _dbContext.SaveChanges();
                }
                return 0;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
        public int UpdateUserPassword(User u)
        {
            try
            {
                var user = getUserByEmail(u.Email);
                if (user != null)
                {
                    user.UserPassword = Utility.encrytoStringKey(u.UserPassword);
                    return _dbContext.SaveChanges();
                }
                return 0;
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }

        }
        public int AddProductToCart(int userID, int productID,int quantity)
        {
            try
            {
                //Checking the cart is existed or not
                var order = _dbContext.Orders.FirstOrDefault(o => o.UserId == userID && o.OrderStatus == "Cart");
                //if not, i will create a new cart
                if(order == null)
                {
                    try
                    {
                        //Setting some temp information i get from user
                        Order o = new Order();
                        o.UserId = userID;
                        o.OrderStatus = "Cart";
                        o.OrderDate = DateTime.Now;
                        o.OrderName = this.GetUserById(userID).UserName;
                        _dbContext.Add(o);
                        _dbContext.SaveChanges();
                        //Now i use recursion to go back 
                        AddProductToCart(userID, productID, quantity);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                //in case thhe cart existed
                else
                {
                    // find the product information
                    var product = _dbContext.Products.First(p => p.CageId == productID);
                    //get information about the cart
                    var orderDetail = _dbContext.OrderDetails.FirstOrDefault(od => od.CageId == productID && od.OrderId == order.OrderId);
                    //take the price not include discount
                    if (orderDetail != null)
                    {
                        int bdb = (int)orderDetail.DetailQuantity;
                        orderDetail.DetailQuantity += 1;
                        var productCartList = this.getListcartByUserID(userID).ToList();
                        order.OrderPrice = productCartList.Sum(p => p.DetailPrice);
                        return _dbContext.SaveChanges();
                    }
                    else
                    {
                        // You need to initialize orderDetail before you can use it
                        OrderDetail od = new OrderDetail();
                        od.OrderId = order.OrderId;
                        od.CageId = productID;
                        od.DetailPrice = product.Price;
                        quantity = 1;
                        od.DetailQuantity = quantity;
                        _dbContext.OrderDetails.Add(od);
                        var productCartList = this.getListcartByUserID(userID).ToList();
                        order.OrderPrice = productCartList.Sum(p => p.DetailPrice);
                        return _dbContext.SaveChanges();
                    }
                }
                return 0;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public List<OrderDetail> getListcartByUserID(int userID)
        {
            try
            {
                var cart = _dbContext.Orders.FirstOrDefault(o => o.UserId == userID && o.OrderStatus == "Cart");    
                if (cart != null)
                {
                    int orderID = cart.OrderId;
                    var result = _dbContext.OrderDetails.Include(o => o.Order).Include(od => od.Cage).Where(o => o.OrderId == orderID).ToList();
                    return result.ToList();

                }
                else
                {
                    return new List<OrderDetail>();
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        
        public Order getOrderPrice_Cart_ByUserID(int userID)
        {
            try
            {
                return _dbContext.Orders.FirstOrDefault(o => o.UserId == userID && o.OrderStatus == "Cart");
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }


        public List<Order> getOrderByUser(int userID)
        {
            try
            {
                return _dbContext.Orders.Where(o => o.OrderStatus != "Cart").ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int UpdateProductInCartByProductID(int userID, int productID, int quantity)
        {
            try
            {
                var order = _dbContext.Orders.FirstOrDefault(o => o.UserId == userID && o.OrderStatus == "Cart");
                if (order != null)
                {
                    var productInCart = _dbContext.OrderDetails.FirstOrDefault(od => od.OrderId == order.OrderId && od.CageId == productID);
                    if (productInCart != null)
                    {
                        productInCart.DetailQuantity = quantity;
                        var product = this.getListcartByUserID(userID).ToList();
                        order.OrderPrice = product.Sum(p => p.DetailPrice);
                        return _dbContext.SaveChanges();
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public int DeleteProductInCartByProductID(int userID, int productID)
        {
            try
            {
                var order = _dbContext.Orders.FirstOrDefault(o => o.UserId == userID && o.OrderStatus == "Cart");
                if (order != null)
                {
                    var productInCart = _dbContext.OrderDetails.FirstOrDefault(od => od.OrderId == order.OrderId && od.CageId == productID);
                    if (productInCart != null)
                    {
                        _dbContext.OrderDetails.Remove(productInCart);
                        var product = this.getListcartByUserID(userID).ToList();
                        order.OrderPrice = product.Sum(p => p.DetailPrice);
                        return _dbContext.SaveChanges();
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        
        


    }
}
