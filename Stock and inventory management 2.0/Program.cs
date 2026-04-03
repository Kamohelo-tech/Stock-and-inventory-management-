using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Stock_and_inventory_management.Program;

namespace Stock_and_inventory_management
{



    // Represents a product in the inventory
    public class Product
        {
            public int Id { get; set; } // Unique identifier for the product
        public string Name { get; set; } // Name of the product
        public decimal Price { get; set; } // Price of the product
        public int StockLevel { get; set; } //current stock level
        public int LowStockThreshold { get; set; } // Threshold for low stock alert

        public bool UpdateStock(int quantity)
            {
                if (StockLevel + quantity < 0)
                {
                    Console.WriteLine("Error: Stock cannot go below zero.");
                    return false;
                }
                StockLevel += quantity;
                return true;
            }
        //Returns a string representation of the product
        public override string ToString()
        {
            return $"ID: {Id} | Name: {Name} | Price: {Price:C} | Stock: {StockLevel} | Low Stock Threshold: {LowStockThreshold}";
        }
    }
    // Represents the inventory system that manages products
    public class Inventory
        {
        //Internal list of products
        private List<Product> products;

        //constructor initializes the product list
        public Inventory()
            {

            products = new List<Product>();

            }
        // Adds a new product to the inventory
        public bool AddStock(Product product) 
            {
            // Checks if product is valid
            if (product == null || product.Name == null)
                {
                    Console.WriteLine("Error: Product cannot be empty.");
                    return false;
                }
            //adds the product to the inventory
            products.Add(product);
                return true;
            }
        // Updates stock of an existing product by ID
        public bool AddStock(int id, int quantity)
            {
            // Finds the product by ID
            Product product = FindProduct(id);
                if (product == null)
                {
                    Console.WriteLine("Error: Product not found.");
                    return false;
                }
                //updates stock level
                return product.UpdateStock(quantity);
            }

        //Removes a product from the inventory by ID
        public bool RemoveProduct(int id)
            {
            //finds the product
                Product product = FindProduct(id);
                if (product == null)
                {
                    Console.WriteLine("Error: Product not found.");
                    return false;
                }
                products.Remove(product);
                return true;
            }

            public Product FindProduct(int id)
            {
                foreach (var product in products)
                {
                    if (product.Id == id)
                        return product;
                }
                return null;
            }
        //Returns a list of all products in the inventory
        public List<Product> GetAllProducts()
            {
                return products;
            }
        //Checks if any product is below threshold
            public bool HasLowStock()
            {
                foreach (var product in products)
                {
                    if (product.StockLevel < product.LowStockThreshold)
                        return true;
                }
                return false;
            }
        }
    // Represents a single sale of a product
    public class SalesRecord
    {
        public DateTime SaleDate { get; set; }     // Date and time of the sale
        public int ProductId { get; set; }         // Product ID sold
        public int QuantitySold { get; set; }      // Number of units sold
        public decimal TotalAmount { get; set; }   // Total sale amount (Price * Quantity)

        // Constructor initializes sale record with current date/time
        public SalesRecord(int productId, int quantitySold, decimal price)
        {
            SaleDate = DateTime.Now;
            ProductId = productId;
            QuantitySold = quantitySold;
            TotalAmount = price * quantitySold;
        }

        // Custom string output for printing the sale details
        public override string ToString()
        {
            return $"{SaleDate:yyyy-MM-dd | HH:mm:ss} | Product ID: {ProductId} | Qty: {QuantitySold} | Total: {TotalAmount:C}";
        }
    }

    // Interface for report generation
    public interface IReport
    {
        void GenerateReport(List<SalesRecord> sales);
    }

    // Generates daily report of all sales made today
    public class DailyReport : IReport
    {


        // Filter only sales that occurred today
        public void GenerateReport(List<SalesRecord> sales)
        {
            Console.WriteLine("\n--- Daily Sales Report ---");

            var todaySales = sales.Where(s => s.SaleDate.Date == DateTime.Today).ToList();

            if (!todaySales.Any())
            {
                Console.WriteLine("No sales today.");
                return;
            }

            decimal totalRevenue = 0;
            int totalQty = 0;

            // Display each sale and calculate totals
            foreach (var sale in todaySales)
            {
                Console.WriteLine(sale);
                totalRevenue += sale.TotalAmount;
                totalQty += sale.QuantitySold;
            }

            Console.WriteLine($"Total items sold today: {totalQty}");
            Console.WriteLine($"Total revenue today: {totalRevenue:C}");
        }
    }


    // Generates weekly report of all sales made in current week (Monday-Sunday)
    public class WeeklyReport : IReport
    {
        public void GenerateReport(List<SalesRecord> sales)
        {
            Console.WriteLine("\n--- Weekly Sales Report ---");

            DateTime today = DateTime.Today;

            // Calculate Monday of the current week
            int diff = DayOfWeek.Monday - today.DayOfWeek;
            if (diff > 0) diff -= 7; // adjust if today is Sunday
            DateTime weekStart = today.AddDays(diff);
            DateTime weekEnd = weekStart.AddDays(6); // Sunday

            // Filter sales between Monday and Sunday
            var weeklySales = sales.Where(s => s.SaleDate.Date >= weekStart && s.SaleDate.Date <= weekEnd).ToList();

            if (!weeklySales.Any())
            {
                Console.WriteLine("No sales this week.");
                return;
            }

            decimal totalRevenue = 0;
            int totalQty = 0;

            foreach (var sale in weeklySales)
            {
                Console.WriteLine(sale);
                totalRevenue += sale.TotalAmount;
                totalQty += sale.QuantitySold;
            }

            Console.WriteLine($"Total items sold this week: {totalQty}");
            Console.WriteLine($"Total revenue this week: {totalRevenue:C}");
        }
    }

    // ======== LOW STOCK NOTIFIER ========
    // Uses a delegate to notify when product stock is low
    public delegate void LowStockEventHandler(Product product);

    public class LowStockNotifier
    {
        private Inventory _inventory;
        private bool _running;
        public event LowStockEventHandler OnLowStock; // Event triggered on low stock

        public LowStockNotifier(Inventory inventory)
        {
            _inventory = inventory;
        }

        // Start monitoring in a separate thread
        public void Start()
        {
            _running = true;
            Task.Run(() =>
            {
                while (_running)
                {
                    foreach (var product in _inventory.GetAllProducts())
                    {
                        if (product.StockLevel < product.LowStockThreshold)
                        {
                            OnLowStock?.Invoke(product); // Trigger low stock event
                        }
                    }
                    Thread.Sleep(5000); // Check every 5 seconds
                }
            });
        }

        public void Stop()
        {
            _running = false;
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Inventory inventory = new Inventory(); // Initialize inventory
            List<SalesRecord> sales = new List<SalesRecord>(); // Initialize sales history

            // Start low stock notifier
            LowStockNotifier notifier = new LowStockNotifier(inventory);
            notifier.OnLowStock += (product) => Console.WriteLine($"⚠ ALERT: {product.Name} is low on stock!");
            notifier.Start();

            bool running = true;

            while (running)
            {
                // Display menu to the user
                Console.WriteLine("\n--- MENU ---");
                Console.WriteLine("1. Add Product");
                Console.WriteLine("2. Update Stock");
                Console.WriteLine("3. Remove Product");
                Console.WriteLine("4. Record Sale");
                Console.WriteLine("5. View Inventory");
                Console.WriteLine("6. Display Daily Report");
                Console.WriteLine("7. Display Weekly Report");
                Console.WriteLine("8. Exit");

                Console.Write("Choose: ");
                string choice = Console.ReadLine();
                Console.Clear();

                try
                {

                    switch (choice)
                    {
                        case "1": // Add new product
                            Console.Write("Enter ID: ");
                            int id = int.Parse(Console.ReadLine());
                            Console.Write("Enter Name: ");
                            string name = Console.ReadLine();
                            Console.Write("Enter Price: ");
                            decimal price = decimal.Parse(Console.ReadLine());
                            Console.Write("Enter Stock: ");
                            int stock = int.Parse(Console.ReadLine());
                            Console.Write("Enter Low Stock Threshold: ");
                            int threshold = int.Parse(Console.ReadLine());


                            inventory.AddStock(new Product
                            {
                                Id = id,
                                Name = name,
                                Price = price,
                                StockLevel = stock,
                                LowStockThreshold = threshold
                            });
                            Console.Clear();
                            break;

                        case "2": // Update existing stock
                            Console.Write("Enter Product ID: ");
                            int updateId = int.Parse(Console.ReadLine());
                            Console.Write("Enter Quantity Change (+/-): ");
                            int qty = int.Parse(Console.ReadLine());
                            inventory.AddStock(updateId, qty);
                            Console.Clear();
                            break;

                        case "3": // Remove a product
                            Console.Write("Enter Product ID: ");
                            int removeId = int.Parse(Console.ReadLine());
                            inventory.RemoveProduct(removeId);
                            Console.Clear();
                            break;

                        case "4": // Record a sale
                            Console.Write("Enter Product ID: ");
                            int saleId = int.Parse(Console.ReadLine());
                            Console.Write("Enter Quantity Sold: ");
                            int saleQty = int.Parse(Console.ReadLine());

                            var product = inventory.FindProduct(saleId);

                            if (product != null && product.UpdateStock(-saleQty))
                            {
                                var sale = new SalesRecord(saleId, saleQty, product.Price);
                                sales.Add(sale);
                                Console.WriteLine("Sale recorded.");
                            }
                            else
                            {
                                Console.WriteLine("Sale could not be recorded.");
                            }
                            Console.Clear();
                            break;

                        case "5": // View all inventory
                            foreach (var p in inventory.GetAllProducts())
                                Console.WriteLine(p);
                            break;

                        case "6": // Generate daily report
                            Task.Run(() => new DailyReport().GenerateReport(sales));
                            break;

                        case "7": // Generate weekly report
                            new WeeklyReport().GenerateReport(sales);
                            break;

                        case "8": // Exit program
                            running = false;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message); // Catch any input/runtime errors
                }
            }
            notifier.Stop();
        }
        
        
    }
    
}
