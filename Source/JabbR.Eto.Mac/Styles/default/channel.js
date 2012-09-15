
var JabbREto = (function(){

	function collapseNotifications($notification) {
		var $notifications = $notification.prevUntil(':not(.notification)');
		if ($notifications.length > 3) {
			
			$notifications.hide().find('.collapse-info').remove();
			
			$notification.find('.collapse-info')
				.text('(plus ' + $notifications.length + ' hidden... click to expand)')
				.removeClass('notification-collapse')
				.addClass('notification-expand');
		}
	}
	
	function expandNotifications($notification) {
		var $notifications = $notification.prevUntil(':not(.notification)');
		//var topBefore = $notification.position().top;
		
		$notification.find('.collapse-info')
			.text('(click to collapse)')
			.removeClass('notification-expand')
			.addClass('notification-collapse');
		
		$notifications.show();
		/*var room = getCurrentRoomElements(),
			topAfter = $notification.position().top,
			scrollTop = room.messages.scrollTop();
		
		// make sure last notification is visible
		room.messages.scrollTop(scrollTop + topAfter - topBefore + $notification.height());
		*/
	}

var pub = {
	messages: [],
	initialize: function() {
		var m = $('#messages');

		m.on('click', '.collapsible_title', function() {
			var box = $(this).next('.collapsible_box');
			box.toggleClass('hidden');
		});
		
		m.on('click', '.notification-expand', function(event) {
			event.preventDefault();
			var notification = $(this).closest('.notification');
			expandNotifications(notification);
		});
		m.on('click', '.notification-collapse', function(event) {
			event.preventDefault();
			var notification = $(this).closest('.notification');
			collapseNotifications(notification);
		});
	},
	addHistory: function(messages) {
		var msgContent = this.translateContent($( "#template-message").render(messages));

		var m = $('#messages');
		this.messages.splice(0, messages);
		m.prepend(msgContent);
		this.scrollToBottom();
	},
	addMessage: function(msg) {
		var msgContent = this.translateContent($( "#template-message").render(msg));
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			existingMsg.replaceWith(msgContent);
			return;
		}
		
		var m = $('#messages');
		this.messages.splice(this.messages.length, msg);
		m.append(msgContent);
		this.scrollToBottom();
	},
	
	translateContent: function (contentString) {
		var content = $(contentString);
		content.find('img').each(function () {
			var img = $(this);
			img.attr('src', img.attr('src').replace(/^proxy[?]url[=]/, ''));
		});
		return content;
	},
	
	addNotification: function(msg) {
		var msgContent = $(this.translateContent($( "#template-notification").render(msg)));

		var m = $('#messages');
		this.messages.splice(this.messages.length, msg);
		m.append(msgContent);
		collapseNotifications(msgContent);
		this.scrollToBottom();
	},

	addMeMessage: function(msg) {
		var msgContent = this.translateContent($( "#template-memessage").render(msg));

		var m = $('#messages');
		this.messages.splice(this.messages.length, msg);
		m.append(msgContent);
		this.scrollToBottom();
	},
	

	addMessageContent: function(msg) {
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			var msgContent = this.translateContent(msg.Content);
			existingMsg.append(msgContent);
			
			var m = $('#messages');
			var scrollHeight = m.prop('scrollHeight');
			var pos = existingMsg.position();
			if (pos.top + existingMsg.outerHeight() < scrollHeight + m.outerHeight())
				this.scrollToBottom();
		}
	},
	scrollToBottom: function() {
		var m = $('#messages');
		m.prop({ scrollTop: m.prop('scrollHeight') });
	}
};

return pub;

}());

$(function() {
	JabbREto.initialize();
})