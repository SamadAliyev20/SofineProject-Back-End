using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using SofineProject.DataAccessLayer;
using SofineProject.Models;
using SofineProject.ViewModels;
using SofineProject.ViewModels.BasketViewModels;
using SofineProject.ViewModels.OrderViewModels;
using System.Data;

namespace SofineProject.Controllers
{
    [Authorize(Roles = "Member")]
    public class OrderController : Controller
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _appDbContext;
        private readonly SmtpSetting _smtpSetting;

        public OrderController(UserManager<AppUser> userManager, AppDbContext appDbContext, IOptions<SmtpSetting> smtpSetting)
        {
            _userManager = userManager;
            _appDbContext = appDbContext;
            _smtpSetting = smtpSetting.Value;
        }
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return RedirectToAction("Index", "Shop");
            }

            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);

            AppUser appUser = await _userManager.Users.Include(u => u.Addresses.Where(u => u.IsMain && u.IsDeleted == false))
                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Address mainAddress = appUser.Addresses.FirstOrDefault();

            if (mainAddress == null)
            {
				TempData["ToasterMessage5"] = "Your address is empty, please add an address and then checkout!";
				return RedirectToAction("Profile", "Account");
            }

            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _appDbContext.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);

                basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                basketVM.Title = product.Title;
            }

            Order order = new Order
            {
                Name = appUser?.Name,
                SurName = appUser?.SurName,
                Email = appUser?.Email,
                Phone = appUser?.PhoneNumber,
                AddressLine = mainAddress?.AddressLine,
                City = mainAddress?.City,
                Country = mainAddress?.Country,
                PostalCode = mainAddress?.PostalCode,
                State = mainAddress?.State,
            };

            OrderVM orderVM = new OrderVM
            {
                Order = order,
                BasketVMs = basketVMs,
            };

            return View(orderVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Checkout(Order order)
        {
            string cookie = HttpContext.Request.Cookies["basket"];
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return RedirectToAction("Index", "Shop");

            }
            AppUser appUser = await _userManager.Users
                .Include(u => u.Baskets.Where(b => b.IsDeleted == false))
                .Include(u => u.Orders)
                .Include(u => u.Addresses.Where(u => u.IsMain && u.IsDeleted == false))
               .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<BasketVM> basketVMs = JsonConvert.DeserializeObject<List<BasketVM>>(cookie);
            foreach (BasketVM basketVM in basketVMs)
            {
                Product product = await _appDbContext.Products.FirstOrDefaultAsync(p => p.Id == basketVM.Id);

                basketVM.Price = product.DiscountedPrice > 0 ? product.DiscountedPrice : product.Price;
                basketVM.Title = product.Title;
            }
            OrderVM orderVM = new OrderVM
            {
                Order = order,
                BasketVMs = basketVMs


            };
            if (!ModelState.IsValid) { return View(order); }

            List<OrderItem> orderItems = new List<OrderItem>();

            foreach (BasketVM basketVM in basketVMs)
            {
                OrderItem orderItem = new OrderItem
                {
                    Count = basketVM.Count,
                    ProductId = basketVM.Id,
                    Price = basketVM.Price,
                    CreatedAt = DateTime.UtcNow.AddHours(4),
                    CreatedBy = $"{appUser.Name} {appUser.SurName}"
                };
                orderItems.Add(orderItem);
            }

            foreach (Basket basket in appUser.Baskets)
            {
                basket.IsDeleted = true;
            }

            HttpContext.Response.Cookies.Append("basket", "");

            order.UserId = appUser.Id;
            order.CreatedAt = DateTime.UtcNow.AddHours(4);
            order.CreatedBy = $"{appUser.Name} {appUser.SurName}";
            order.OrderItems = orderItems;
            order.No = appUser.Orders != null && appUser.Orders.Count() > 0 ? appUser.Orders.Last().No + 1 : appUser.Orders.Last().No + 1;

            await _appDbContext.Orders.AddAsync(order);
            await _appDbContext.SaveChangesAsync();
            string tempalteFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templaes", "_CheckoutPartialForEmail.html");

            string templateContent = await System.IO.File.ReadAllTextAsync(tempalteFullPath);
            string adress = $"{appUser.Addresses.FirstOrDefault().State} {appUser.Addresses.FirstOrDefault().City} {appUser.Addresses.FirstOrDefault().Country}";
            string productList = $"";
            string url = Url.Action("Index", "Home");

            foreach (OrderItem orderItem in orderItems)
            {
                productList += $"<div style=\"background-color:transparent\">\r\n              <div style=\"min-width:320px;max-width:680px;word-wrap:break-word;word-break:break-word;margin:0 auto;background-color:transparent\">\r\n                <div style=\"border-collapse:collapse;display:table;width:100%;background-color:transparent\">\r\n                  \r\n                  \r\n                  <div style=\"display:table-cell;vertical-align:top;max-width:320px;min-width:224px;width:226px\">\r\n                    <div style=\"width:100%!important\">\r\n                      \r\n                      <div style=\"border-top:0px solid transparent;border-left:0px solid transparent;border-bottom:0px solid transparent;border-right:0px solid transparent;padding-top:5px;padding-bottom:5px;padding-right:5px;padding-left:5px\">\r\n                        \r\n                        \r\n                        <div style=\"color:#393d47;font-family:Nunito,Arial,Helvetica Neue,Helvetica,sans-serif;line-height:1.2;padding-top:10px;padding-right:0px;padding-bottom:10px;padding-left:10px\">\r\n                          <div style=\"line-height:1.2;font-size:12px;color:#393d47;font-family:Nunito,Arial,Helvetica Neue,Helvetica,sans-serif\">\r\n                            <p style=\"font-size:14px;line-height:1.2;word-break:break-word;margin:0\">\r\n                              {basketVMs.FirstOrDefault(o => o.Id == orderItem.ProductId).Title}\r\n                            </p>\r\n                          </div>\r\n                        </div>\r\n                        \r\n                        \r\n                      </div>\r\n                      \r\n                    </div>\r\n                  </div>\r\n                  \r\n                  \r\n                  <div style=\"display:table-cell;vertical-align:top;max-width:320px;min-width:224px;width:226px\">\r\n                    <div style=\"width:100%!important\">\r\n                      \r\n                      <div style=\"border-top:0px solid transparent;border-left:0px solid transparent;border-bottom:0px solid transparent;border-right:0px solid transparent;padding-top:5px;padding-bottom:5px;padding-right:5px;padding-left:5px\">\r\n                        \r\n                        \r\n                        <div style=\"color:#393d47;font-family:Nunito,Arial,Helvetica Neue,Helvetica,sans-serif;line-height:1.2;padding-top:10px;padding-right:5px;padding-bottom:10px;padding-left:5px\">\r\n                          <div style=\"line-height:1.2;font-size:12px;color:#393d47;font-family:Nunito,Arial,Helvetica Neue,Helvetica,sans-serif\">\r\n                            <p style=\"font-size:14px;line-height:1.2;word-break:break-word;text-align:center;margin:0\">\r\n                              {orderItem.Count}\r\n                            </p>\r\n                          </div>\r\n                        </div>\r\n\r\n                      </div>\r\n\r\n                    </div>\r\n                  </div>\r\n\r\n                  <div style=\"display:table-cell;vertical-align:top;max-width:320px;min-width:224px;width:226px\">\r\n                    <div style=\"width:100%!important\">\r\n                      \r\n                      <div style=\"border-top:0px solid transparent;border-left:0px solid transparent;border-bottom:0px solid transparent;border-right:0px solid transparent;padding-top:5px;padding-bottom:5px;padding-right:5px;padding-left:5px\">\r\n                        \r\n                        \r\n                        <div style=\"color:#393d47;font-family:Nunito,Arial,Helvetica Neue,Helvetica,sans-serif;line-height:1.2;padding-top:10px;padding-right:10px;padding-bottom:10px;padding-left:0px\">\r\n                          <div style=\"line-height:1.2;font-size:12px;color:#393d47;font-family:Nunito,Arial,Helvetica Neue,Helvetica,sans-serif\">\r\n                            <p style=\"font-size:14px;line-height:1.2;word-break:break-word;text-align:right;margin:0\">\r\n                              ${orderItem.Price * orderItem.Count}\r\n                            </p>\r\n                          </div>\r\n                        </div>\r\n                        \r\n                        \r\n                      </div>\r\n                      \r\n                    </div>\r\n                  </div>\r\n                  \r\n                  \r\n                </div>\r\n              </div>\r\n            </div>  <div style=\"background-color: transparent\">\r\n              <div\r\n                class=\"block-grid\"\r\n                style=\"\r\n                  min-width: 320px;\r\n                  max-width: 680px;\r\n                  overflow-wrap: break-word;\r\n                  word-wrap: break-word;\r\n                  word-break: break-word;\r\n                  margin: 0 auto;\r\n                  background-color: transparent;\r\n                \"\r\n              >\r\n                <div\r\n                  style=\"\r\n                    border-collapse: collapse;\r\n                    display: table;\r\n                    width: 100%;\r\n                    background-color: transparent;\r\n                  \"\r\n                >\r\n                  <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background-color:transparent;\"><tr><td align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:680px\"><tr class=\"layout-full-width\" style=\"background-color:transparent\"><![endif]-->\r\n                  <!--[if (mso)|(IE)]><td align=\"center\" width=\"680\" style=\"background-color:transparent;width:680px; border-top: 0px solid transparent; border-left: 0px solid transparent; border-bottom: 0px solid transparent; border-right: 0px solid transparent;\" valign=\"top\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding-right: 0px; padding-left: 0px; padding-top:5px; padding-bottom:5px;\"><![endif]-->\r\n                  <div\r\n                    class=\"col num12\"\r\n                    style=\"\r\n                      min-width: 320px;\r\n                      max-width: 680px;\r\n                      display: table-cell;\r\n                      vertical-align: top;\r\n                      width: 680px;\r\n                    \"\r\n                  >\r\n                    <div class=\"col_cont\" style=\"width: 100% !important\">\r\n                      <!--[if (!mso)&(!IE)]><!-->\r\n                      <div\r\n                        style=\"\r\n                          border-top: 0px solid transparent;\r\n                          border-left: 0px solid transparent;\r\n                          border-bottom: 0px solid transparent;\r\n                          border-right: 0px solid transparent;\r\n                          padding-top: 5px;\r\n                          padding-bottom: 5px;\r\n                          padding-right: 0px;\r\n                          padding-left: 0px;\r\n                        \"\r\n                      >\r\n                        <!--<![endif]-->\r\n                        <table\r\n                          border=\"0\"\r\n                          cellpadding=\"0\"\r\n                          cellspacing=\"0\"\r\n                          class=\"divider\"\r\n                          role=\"presentation\"\r\n                          style=\"\r\n                            table-layout: fixed;\r\n                            vertical-align: top;\r\n                            border-spacing: 0;\r\n                            border-collapse: collapse;\r\n                            mso-table-lspace: 0pt;\r\n                            mso-table-rspace: 0pt;\r\n                            min-width: 100%;\r\n                            -ms-text-size-adjust: 100%;\r\n                            -webkit-text-size-adjust: 100%;\r\n                          \"\r\n                          valign=\"top\"\r\n                          width=\"100%\"\r\n                        >\r\n                          <tbody>\r\n                            <tr style=\"vertical-align: top\" valign=\"top\">\r\n                              <td\r\n                                class=\"divider_inner\"\r\n                                style=\"\r\n                                  word-break: break-word;\r\n                                  vertical-align: top;\r\n                                  min-width: 100%;\r\n                                  -ms-text-size-adjust: 100%;\r\n                                  -webkit-text-size-adjust: 100%;\r\n                                  padding-top: 0px;\r\n                                  padding-right: 0px;\r\n                                  padding-bottom: 0px;\r\n                                  padding-left: 0px;\r\n                                \"\r\n                                valign=\"top\"\r\n                              >\r\n                                <table\r\n                                  align=\"center\"\r\n                                  border=\"0\"\r\n                                  cellpadding=\"0\"\r\n                                  cellspacing=\"0\"\r\n                                  class=\"divider_content\"\r\n                                  height=\"1\"\r\n                                  role=\"presentation\"\r\n                                  style=\"\r\n                                    table-layout: fixed;\r\n                                    vertical-align: top;\r\n                                    border-spacing: 0;\r\n                                    border-collapse: collapse;\r\n                                    mso-table-lspace: 0pt;\r\n                                    mso-table-rspace: 0pt;\r\n                                    border-top: 1px solid #e1ecef;\r\n                                    height: 1px;\r\n                                    width: 100%;\r\n                                  \"\r\n                                  valign=\"top\"\r\n                                  width=\"100%\"\r\n                                >\r\n                                  <tbody>\r\n                                    <tr\r\n                                      style=\"vertical-align: top\"\r\n                                      valign=\"top\"\r\n                                    >\r\n                                      <td\r\n                                        height=\"1\"\r\n                                        style=\"\r\n                                          word-break: break-word;\r\n                                          vertical-align: top;\r\n                                          -ms-text-size-adjust: 100%;\r\n                                          -webkit-text-size-adjust: 100%;\r\n                                        \"\r\n                                        valign=\"top\"\r\n                                      >\r\n                                        <span></span>\r\n                                      </td>\r\n                                    </tr>\r\n                                  </tbody>\r\n                                </table>\r\n                              </td>\r\n                            </tr>\r\n                          </tbody>\r\n                        </table>\r\n                        <!--[if (!mso)&(!IE)]><!-->\r\n                      </div>\r\n                      <!--<![endif]-->\r\n                    </div>\r\n                  </div>\r\n                  <!--[if (mso)|(IE)]></td></tr></table><![endif]-->\r\n                  <!--[if (mso)|(IE)]></td></tr></table></td></tr></table><![endif]-->\r\n                </div>\r\n              </div>\r\n            </div>";
            }

            templateContent = templateContent.Replace("{{Adress}}", adress);
            templateContent = templateContent.Replace("{{OrderNo}}", $"{order.No}");
            templateContent = templateContent.Replace("{{CreatedAt}}", $"{order.CreatedAt}");
            templateContent = templateContent.Replace("{{checkOutProducts}}", productList);
            templateContent = templateContent.Replace("{{Total}}", $"{basketVMs.Sum(b => b.Price * b.Count)}");
            templateContent = templateContent.Replace("{{Link}}", url);



            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(_smtpSetting.Email));
            mimeMessage.To.Add(MailboxAddress.Parse(appUser.Email));
            mimeMessage.Subject = "Check Out";
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = templateContent
            };
            using (SmtpClient smtpClient = new SmtpClient())
            {
                smtpClient.CheckCertificateRevocation = false;
                await smtpClient.ConnectAsync(_smtpSetting.Host, _smtpSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_smtpSetting.Email, _smtpSetting.Password);
                await smtpClient.SendAsync(mimeMessage);
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }
            TempData["ToasterMessage"] = "Order Placed successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}
