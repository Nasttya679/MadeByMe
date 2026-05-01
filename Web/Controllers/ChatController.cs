using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs.Chat;
using MadeByMe.Application.Services.Interfaces;
using MadeByMe.Web.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MadeByMe.Web.Controllers
{
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _chatService.GetUserChatsAsync(CurrentUserId!);
            if (result.IsFailure)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction("Index", "Home");
            }

            return View(result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Room(int id)
        {
            var messagesResult = await _chatService.GetChatMessagesAsync(id, CurrentUserId!);

            var chatsResult = await _chatService.GetUserChatsAsync(CurrentUserId!);

            if (messagesResult.IsFailure || chatsResult.IsFailure)
            {
                SetErrorMessage("Не вдалося завантажити чат.");
                return RedirectToAction("Index");
            }

            ViewBag.CurrentChatId = id;
            ViewBag.AllChats = chatsResult.Value;

            var currentChat = chatsResult.Value.FirstOrDefault(c => c.ChatId == id);
            ViewBag.InterlocutorName = currentChat?.InterlocutorName;
            ViewBag.InterlocutorPhoto = currentChat?.InterlocutorPhoto;
            ViewBag.InterlocutorId = currentChat?.InterlocutorId;

            ViewData["Title"] = $"Повідомлення - {currentChat?.InterlocutorName}";

            return View(messagesResult.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrCreateChat(string targetUserId)
        {
            if (string.IsNullOrEmpty(targetUserId))
            {
                return BadRequest();
            }

            var result = await _chatService.GetOrCreateChatAsync(CurrentUserId!, targetUserId);

            if (result.IsSuccess)
            {
                return RedirectToAction("Room", new { id = result.Value.Id });
            }

            SetErrorMessage(result.ErrorMessage);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage([FromForm] ChatMessageDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некоректні дані");
            }

            var result = await _chatService.SendMessageAsync(CurrentUserId!, dto);

            if (result.IsSuccess)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                var jsonMessage = JsonConvert.SerializeObject(result.Value, settings);

                await _hubContext.Clients.Group(dto.ChatId.ToString())
                    .SendAsync("ReceiveMessage", jsonMessage);

                return Ok();
            }

            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> TogglePin(int chatId)
        {
            var result = await _chatService.TogglePinChatAsync(chatId, CurrentUserId!);
            if (result.IsFailure)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int messageId, bool forEveryone)
        {
            var result = await _chatService.DeleteMessageAsync(messageId, CurrentUserId!, forEveryone);

            if (result.IsSuccess)
            {
                if (forEveryone)
                {
                    await _hubContext.Clients.All.SendAsync("MessageDeleted", messageId);
                }

                return Ok();
            }

            return BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteChat(int chatId, bool forEveryone)
        {
            var result = await _chatService.DeleteChatAsync(chatId, CurrentUserId!, forEveryone);

            if (result.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(result.ErrorMessage);
        }
    }
}