using Microsoft.AspNetCore.Mvc;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;
using MyShop.ViewModels;
using MyShop.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyShop.Controllers;

public class OrderController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly IItemRepository _itemRepository;

    public OrderController(IOrderRepository orderRepository, IItemRepository itemRepository)
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
    }

    public async Task<IActionResult> Table()
    {
        List<Order> orders = await _orderRepository.GetAll();
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> CreateOrderItem()
    {
        var items = await _itemRepository.GetAll(); // should i use my item repository or a new method in my order repo?
        var orders = await _orderRepository.GetAll();
        var createOrderItemViewModel = new CreateOrderItemViewModel
        {
            OrderItem = new OrderItem(),

            ItemSelectList = items.Select(item => new SelectListItem
            {
                Value = item.ItemId.ToString(),
                Text = item.ItemId.ToString() + ": " + item.Name
            }).ToList(),

            OrderSelectList = orders.Select(order => new SelectListItem
            {
                Value = order.OrderId.ToString(),
                Text = "Order" + order.OrderId.ToString() + ", Date: " + order.OrderDate + ", Customer: " + order.Customer.Name
            }).ToList(),
        };

        return View(createOrderItemViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderItem(OrderItem orderItem)
    {

        try
        {
            var item  = await _itemRepository.GetItemById(orderItem.ItemId);
            var order = await _orderRepository.GetOrderById(orderItem.OrderId);

            if (item == null || order == null)
            {
                return BadRequest("Item or Order not found.");
            }

            var newOrderItem = new OrderItem
            {
                ItemId = orderItem.ItemId,
                Quantity = orderItem.Quantity,
                OrderId = orderItem.OrderId,
                OrderItemPrice = orderItem.Quantity * item.Price
            };
            await _orderRepository.AddOrderItem(newOrderItem);
            return RedirectToAction(nameof(Table));
        }
        catch
        {
            return BadRequest("OrderItem creation failed.");
        }
    }
}