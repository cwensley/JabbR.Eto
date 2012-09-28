

var JabbREto = (function(){

	var v = {
		scrollPos: 0,
		bottomThreshold: 20,
		scrolls: []
	};

	function shouldScrollToBottom () {
		var bottomPos = v.scrollPos + v.scrollContainer.height();
		return bottomPos > v.scrollContent.height() - v.bottomThreshold;
	}

	function saveScroll(element) {
		var scrollPos = v.scrollPos; // v.scrollContainer.scrollTop()
		if (!element) {
			var bottomPos = scrollPos + v.scrollContainer.height() - v.bottomThreshold;
			//console.log('FINDING: ' + bottomPos);
			element = v.messages.children().filter(function (index) { return ($(this).position().top + $(this).height()) >= bottomPos; }).first();
		}
		if (!element || !element.length) {
			//console.log('USING LAST!');
			element = v.messages.children().last ();
		}
		if (element && element.length) {
			var pos = element.position().top + element.height() - scrollPos;
			//console.log(element.html());
			//console.log('element pos:' + element.position().top + ', pos:' + pos + ', scroll pos:' + scrollPos + ', doc size:' + v.scrollContent.height());
			var scroll = {
				pos: pos,
				element: element
			};
			v.scrolls.push(scroll);
			return scroll;
		}
		return null;
	}
	
	function restoreScroll(scroll)
	{
		if (scroll && scroll.element && scroll.element.length) {
			var oldpos = v.scrollPos;
			v.scrollPos = scroll.element.position().top + scroll.element.height() - scroll.pos;
			
			//console.log('NEW element pos:' + scroll.element.position().top + ', old scroll pos:' + oldpos + ', scroll pos:' + v.scrollPos + ', doc size:' + v.scrollContent.height());
			v.scrollContainer.scrollTop(v.scrollPos);
			v.scrolls.splice($.inArray(scroll, v.scrolls), 1);
		}
	}


	function collapseNotifications($notification, saveScrollPosition) {
		var $notifications = $notification.prevUntil(':not(.notification)');
		if ($notifications.length > 3) {
			
			var scroll = saveScrollPosition ? saveScroll ($notification) : null;
			$notifications.hide().find('.collapse-info').remove();
			
			$notification.find('.collapse-info')
				.text('(plus ' + $notifications.length + ' hidden... click to expand)')
				.removeClass('notification-collapse')
				.addClass('notification-expand');

			if (saveScrollPosition)
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
	
	function imageLoadFix(msgContent) {
		$(msgContent).find('img').each(function() {
			var img = $(this);
			var src = img.attr('src');
			var loaderImg = $("<img>").attr('src', src);
			if (!loaderImg[0].complete) {
				img.attr('src', 'loader.gif');
				loaderImg.load(function() {
					var scroll = saveScroll();
					img.load(function() {
						restoreScroll(scroll);
					});
					img.attr('src', src);
				});
			}
		});
	}

var pub = {
	initialize: function() {
		v.messages = $('#messages');
		v.scrollContainer = $(window);
		v.scrollContent = $(document);
		
		v.scrollContainer.scroll(function() {
			var newScroll = v.scrollContainer.scrollTop ();
			for (var scroll in v.scrolls) {
				scroll.pos += v.scrollPos - newScroll;
			}
			v.scrollPos = newScroll;
			
			if (!v.loading && v.scrollPos == 0) {
				pub.beginLoad ();
				/**/
				window.location.search = '?load-history';
				/**
				setTimeout(function() {
					pub.addHistory([
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'},
						{Time: '10:42:00 AM', User: 'User', Type: 'message', Content: 'Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum'}
					]);
					pub.finishLoad();
				}, 2000);
				/**/
			}
		});

		$(window).resize(function() {
			var topic = $('#topic:visible');
			if (topic.length > 0) {
				var height = topic.outerHeight();
				$('#container').css('padding-top', height + 'px');
			}
		});

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
			collapseNotifications(notification, true);
		});
		v.messages.on('click', 'a[href^="#/rooms/"]', function(event) {
			event.preventDefault ();
			var name = $(this).attr('href').substring(8);
			window.location.search = '?join-room=' + name;
		});
	},
	finishLoad: function() {
		v.loading = false;
		$('#loader').hide();
	},
	beginLoad: function() {
		v.loading = true;
		$('#loader').show();
	},
	addHistory: function(messages, scrollToBottom) {
		var scroll = scrollToBottom ? shouldScrollToBottom() : saveScroll();
		var msgContent = this.translateContent($( "#template-message").render(messages));

		imageLoadFix(msgContent);
		v.messages.prepend(msgContent);
		if (scrollToBottom)
			pub.scrollToBottom(scroll);
		else
			restoreScroll(scroll);
			
	},
	addMessage: function(msg) {
		var scroll = shouldScrollToBottom ();
		var msgContent = this.translateContent($( "#template-message").render(msg));
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			existingMsg.replaceWith(msgContent);
			return;
		}
		imageLoadFix(msgContent);
		v.messages.append(msgContent);
		pub.scrollToBottom(scroll);
	},
	
	translateContent: function (contentString) {
		var content = $(contentString);
		content.find('img').each(function () {
			var img = $(this);
			img.attr('src', img.attr('src').replace(/^proxy[?]url[=]/, ''));
		});
		return content;
	},
	
	addNotification: function(msg, allowCollapsing) {
		var scroll = shouldScrollToBottom();
		var msgContent = $(this.translateContent($( "#template-notification").render(msg)));

		v.messages.append(msgContent);
		if (allowCollapsing)
			collapseNotifications(msgContent, false);
		pub.scrollToBottom(scroll);
	},

	addMeMessage: function(msg) {
		var scroll = shouldScrollToBottom();
		var msgContent = this.translateContent($( "#template-memessage").render(msg));

		v.messages.append(msgContent);
		pub.scrollToBottom(scroll);
	},
	

	addMessageContent: function(msg) {
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			var scroll = saveScroll (existingMsg);
			
			var msgContent = this.translateContent(msg.Content);
			imageLoadFix(msgContent);
			existingMsg.append(msgContent);

			restoreScroll(scroll);
		}
	},
	scrollToBottom: function(shouldScrollToBottom) {
		if (shouldScrollToBottom === undefined || shouldScrollToBottom) {
			v.scrollPos = Math.max(0, $(document).height() - v.scrollContainer.height());
			v.scrollContainer.scrollTop (v.scrollPos);
		}
	},
	setTopic: function(topic) {
		var scroll = saveScroll();
		var $topic = $('#topic');
		var oldHeight = $topic.outerHeight();
		var height = 0;
		if (topic && topic.length) {
			$topic.html(topic).show();
			height = $topic.outerHeight();
		}
		else $topic.hide();
		$('#container').css('padding-top', height + 'px');
		restoreScroll(scroll);
	},
	captureDocumentWrite: function (documentWritePath, headerText, elementToAppendTo) {
        $.fn.captureDocumentWrite(documentWritePath, function (content) {
            var scroll = shouldScrollToBottom(),
                collapsible = null,
                insertContent = null,
                links = null;

            collapsible = $('<div><h3 class="collapsible_title">' + headerText + ' (click to show/hide)</h3><div class="collapsible_box captureDocumentWrite_content"></div></div>');
            $('.captureDocumentWrite_content', collapsible).append(content);

            // Since IE doesn't render the css if the links are not in the head element, we move those to the head element
            links = $('link', collapsible);
            links.remove();
            $('head').append(links);

            // Remove the target off any existing anchor tags, then re-add target as _blank so it opens new tab (or window)
            $('a', collapsible).removeAttr('target').attr('target', '_blank');

            insertContent = collapsible[0].outerHTML;

			// TODO: collapse rich content if setting is set

            elementToAppendTo.append(insertContent);

			pub.scrollToBottom(scroll);
        });
	}
};

return pub;

}());


$(function() {

	JabbREto.initialize();

    window.captureDocumentWrite = function (documentWritePath, headerText, elementToAppendTo) {
    	JabbREto.captureDocumentWrite (documentWritePath, headerText, elementToAppendTo);
    };
    
    window.addTweet = function () {
	};
});