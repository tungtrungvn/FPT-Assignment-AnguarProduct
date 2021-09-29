using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ProductServer.DTOs;
using ProductServer.Models;

namespace ProductServer.Repositories
{
  public class ProductRepository : GenericRepository<Product>
  {
    public ProductRepository(ProductContext context) : base(context) { }

    public IEnumerable<Product> GetAll()
    {
      var products = Entities
      .Include(p => p.Supplier)
      .Include(p => p.Categories)
      .AsNoTracking()
      .Select(p => new Product
      {
        ID = p.ID,
        Name = p.Name,
        Price = p.Price,
        Rating = p.Rating,
        Supplier = new Supplier { Name = p.Supplier.Name },
        Categories = p.Categories.Select(c => new Category { Name = c.Name })
      })
      .AsEnumerable();

      return products;
    }

    public Product GetProduct(Guid id)
    {
      var product = Entities
      .Include(p => p.ProductDetail)
      .Include(p => p.Categories)
      .Include(p => p.Supplier)
      .AsNoTracking()
      .Select(p => new Product
      {
        ID = p.ID,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        Rating = p.Rating,
        ReleaseDate = p.ReleaseDate,
        DiscontinuedDate = p.DiscontinuedDate,
        ProductDetail = p.ProductDetail,
        Supplier = new Supplier { ID = p.Supplier.ID, Name = p.Supplier.Name },
        Categories = p.Categories.Select(c => new Category { ID = c.ID, Name = c.Name })
      })
      .SingleOrDefault(p => p.ID.Equals(id));

      return product;
    }

    public IEnumerable<Product> Find(ProductFilter filter, ProductOrder order, int size, int offset, out int total)
    {
      Func<Product, bool> predicate = (product) =>
      {
        return (product.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)
                || product.Categories.Contains(new Category { Name = filter.Name }, new ContainComparer())
                || product.Supplier.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase)
              )
               && filter.Categories.All(c => product.Categories.Contains(new Category { ID = c }))
               && product.Price >= filter.MinPrice
               && product.Price <= filter.MaxPrice;
      };

      IEnumerable<Product> Sort(IEnumerable<Product> items)
      {
        items = order.Rating == OrderBy.ASC ? items.OrderBy(p => p.Rating) : items.OrderByDescending(p => p.Rating);
        items = order.Price == OrderBy.ASC ? items.OrderBy(p => p.Price) : items.OrderByDescending(p => p.Price);
        items = order.Name == OrderBy.ASC ? items.OrderBy(p => p.Name) : items.OrderByDescending(p => p.Name);

        return items;
      };


      var enumerable = Entities
      .Include(p => p.Supplier)
      .Include(p => p.Categories)
      .AsNoTracking()
      .Where(predicate)
      .Select(p => new Product
      {
        ID = p.ID,
        Name = p.Name,
        Price = p.Price,
        Rating = p.Rating,
        Supplier = new Supplier { Name = p.Supplier.Name },
        Categories = p.Categories.Select(c => new Category { Name = c.Name })
      });

      total = enumerable.Count();

      var products = enumerable.Skip(size * offset).Take(size);
      var sorted = Sort(products);

      return sorted;
    }
  }

  public class ProductFilter
  {
    public string Name { get; set; }
    public List<Guid> Categories { get; set; }
    public double MinPrice { get; set; }
    public double MaxPrice { get; set; }
  }

  public class ProductOrder
  {
    public OrderBy Name { get; set; } = OrderBy.ASC;
    public OrderBy Price { get; set; } = OrderBy.ASC;
    public OrderBy Rating { get; set; } = OrderBy.ASC;

  }

  public enum OrderBy
  {
    ASC,
    DESC
  }
  public class ContainComparer : IEqualityComparer<Category>
  {

    bool IEqualityComparer<Category>.Equals(Category x, Category y)
    {
      return x.Name.Contains(y.Name, StringComparison.OrdinalIgnoreCase);
    }


    int IEqualityComparer<Category>.GetHashCode(Category obj)
    {
      throw new NotImplementedException();
    }
  }
}