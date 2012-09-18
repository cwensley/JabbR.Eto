
var JabbREto = (function(){

	var v = { }

	function saveScroll(element) {
		var pos = (element) ? $(element).position().top + $(element).height() : v.scrollContainer.scrollTop ();
		return {
			pos: pos,
			element: $(element)
		};
	}
	
	function restoreScroll(scroll)
	{
		var pos = scroll.pos;
		if (scroll.element) {
			pos = v.scrollContainer.scrollTop() + scroll.element.position().top - scroll.pos + scroll.element.height();
		}
		v.scrollContainer.scrollTop(pos);
	}


	function collapseNotifications($notification) {
		var $notifications = $notification.prevUntil(':not(.notification)');
		if ($notifications.length > 3) {
			
			var scroll = saveScroll ($notification);
			$notifications.hide().find('.collapse-info').remove();
			
			$notification.find('.collapse-info')
				.text('(plus ' + $notifications.length + ' hidden... click to expand)')
				.removeClass('notification-collapse')
				.addClass('notification-expand');
			restoreScroll (scroll);
		}
	}
	
	function expandNotifications($notification) {
		var $notifications = $notification.prevUntil(':not(.notification)');
		var scroll = saveScroll ($notification);
		
		$notification.find('.collapse-info')
			.text('(click to collapse)')
			.removeClass('notification-expand')
			.addClass('notification-collapse');
		
		$notifications.show();
		
		restoreScroll (scroll);
	}

var pub = {
	initialize: function() {
		v.messages = $('#messages');
		/**/
		v.scrollContainer = $('#container');
		v.scrollContent = v.messages;
		/**
		v.scrollContainer = $(window);
		v.scrollContent = $(document);
		/**/

		v.messages.on('click', '.collapsible_title', function(event) {
			var box = $(this).next('.collapsible_box');
			box.toggleClass('hidden');
		});
		
		v.messages.on('click', '.notification-expand', function(event) {
			event.preventDefault();
			var notification = $(this).closest('.notification');
			expandNotifications(notification);
		});
		v.messages.on('click', '.notification-collapse', function(event) {
			event.preventDefault();
			var notification = $(this).closest('.notification');
			collapseNotifications(notification);
		});
		
		v.messages.on('load', 'img', function(event) {
			this.scrollToBottom();
		});
	},
	addHistory: function(messages) {
		var msgContent = this.translateContent($( "#template-message").render(messages));

		v.messages.prepend(msgContent);
		this.scrollToBottom();
	},
	addMessage: function(msg) {
		var msgContent = this.translateContent($( "#template-message").render(msg));
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			existingMsg.replaceWith(msgContent);
			return;
		}
		$(msgContent).find('img').one('load', function () {
			pub.scrollToBottom ();
		});
		v.messages.append(msgContent);
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

		v.messages.append(msgContent);
		collapseNotifications(msgContent);
		this.scrollToBottom();
	},

	addMeMessage: function(msg) {
		var msgContent = this.translateContent($( "#template-memessage").render(msg));

		v.messages.append(msgContent);
		this.scrollToBottom();
	},
	

	addMessageContent: function(msg) {
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			var scrollHeight = v.scrollContent.prop('scrollHeight');
			var pos = existingMsg.position();
			var shouldScroll = (pos.top + existingMsg.outerHeight() < scrollHeight + v.scrollContainer.outerHeight());
			
			var msgContent = this.translateContent(msg.Content);
			if (shouldScroll) {
				$(msgContent).find('img').one('load', function () {
					pub.scrollToBottom ();
				});
			}
			existingMsg.append(msgContent);

			if (shouldScroll)
				this.scrollToBottom();
		}
	},
	scrollToBottom: function() {
		v.scrollContainer.scrollTop (v.scrollContent.height());
	},
	setTopic: function(topic) {
		var $topic = $('#topic');
		var height = 0;
		if (topic && topic.length) {
			$topic.html(topic).show();
			height = $topic.outerHeight();
		}
		else $topic.hide();
		v.messages.css('padding-top', height + 'px');
	}
};

return pub;

}());

$(function() {
	JabbREto.initialize();
})