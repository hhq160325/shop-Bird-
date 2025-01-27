﻿using BusinessObjects.Models;
using DataAccessObjects;

namespace Repository
{
    public class AccessoryRepository : IAccessoryRepository
    {
        private readonly AccessoryDAO _dao;

        public AccessoryRepository()
        {
            _dao = new AccessoryDAO();
        }
        public void Add(Accessory accessory) => _dao.Add(accessory);

        public void Delete(int id) => _dao.Delete(id);

        public IEnumerable<Accessory> GetAll() => _dao.GetAll();

        public List<Category> GetCategories() => _dao.GetCategories();

        public Accessory GetAccessoryById(int id) => _dao.GetAccessoryById(id);

        public List<Accessory> GetAccessoryByName(string name) => _dao.GetAccessoryByName(name);

        public List<Discount> GetDiscounts() => _dao.GetDiscounts();

        public void Update(Accessory accessory) => _dao.Update(accessory);
        public List<Accessory> getAccessoryPages(int pageIndex, int pageSize) => _dao.getAccessoryPages(pageIndex, pageSize);
        public int getTotalAccessoryPages() => _dao.getTotalAccessoryPages();
    }
}
