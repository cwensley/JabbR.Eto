

var JabbR = (function(){

	var v = {
		scrollPos: 0,
		bottomThreshold: 20,
		scrolls: [],
		addMarker: false,
		settings: {
			html5video: true,
			fullscreenVideo: true,
			showRichContent: true,
			maxHistory: 500
		}
	};
	
	function addParameter(url, param){
		return url + (url.indexOf('?') < 0 ? '?':'&') + param;
	}

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
		msgContent.find('img').each(function() {
			var img = $(this);
			var src = img.attr('src');
			var loaderImg = $("<img>").attr('src', src);
			if (!loaderImg[0].complete) {
				img.attr('src', 'loader.gif');
				loaderImg.one('load', function() {
					var scroll = saveScroll();
					img.one('load', function() {
						restoreScroll(scroll);
					});
					img.attr('src', src);
				});
			}
		});
	}
	
	var reg = /.*(?:youtu.be\/|v\/|u\/\w\/|embed\/|watch\?v=)([^#\&\?]*).*/;
	function youtubeToHtml5(content) {
		content.find('object > embed').each(function() {
			var embed = $(this); //.find('embed');
			var container = embed.closest('object');
			//console.log('found!!');
			var src = embed.attr('src');
			var id = (src.match( reg ) || [])[1];
			if (id) {
				if (v.settings.fullscreenVideo) id = addParameter(id, 'fs=1');
				//if (v.settings.html5video) id = addParameter(id, 'html5=1');
				container.replaceWith('<iframe width="425" height="344" src="http://www.youtube.com/embed/' + id + '" frameborder="0"></iframe>');
			};
		});
	}

	function encodeHtml(html) {
		return $("<div/>").text(html).html();
	}

	function markdownToHtml(markdown) {
		var converter = new Markdown.Converter().makeHtml;
		return converter(markdown);
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
			
			if (!v.loading && newScroll < 10 && v.scrollPos > 10) {
				pub.beginLoad ();
				window.location.search = '?load-history';
			}
			v.scrollPos = newScroll;
		});

		$(window).resize(function() {
			var topic = $('#topic:visible');
			if (topic.length > 0) {
				var height = topic.outerHeight();
				$('#container').css('padding-top', height + 'px');
			}
			//pub.scrollToBottom(true);
			
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
	
	settings: function(settings) {
		$.extend(v.settings, settings);
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

		v.messages.prepend(msgContent);
		if (scrollToBottom)
			pub.scrollToBottom(scroll);
		else
			restoreScroll(scroll);
			
	},

	addMessage: function(msg) {
		if (v.addMarker) {
			if (v.marker)
				v.marker.remove ();
			else
				v.marker = $('<li class="marker"></li>');
			v.messages.append(v.marker);
			v.addMarker = false;
		}			

	
		var scroll = shouldScrollToBottom ();
		var msgContent = this.translateContent($( "#template-message").render(msg));
		
		var existingMsg = $('#m-' + msg.Id);
		if (existingMsg.length > 0) {
			existingMsg.replaceWith(msgContent);
			return;
		}
		v.messages.append(msgContent);
		pub.scrollToBottom(scroll);
	},
	
	translateContent: function (contentString) {
		var content = $(contentString);
		content.find('img').each(function () {
			var img = $(this);
			img.attr('src', img.attr('src').replace(/^proxy[?]url[=]/, ''));
		});
		imageLoadFix(content);
		//youtubeToHtml5(content);
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

	setMarker: function () {
		v.addMarker = true;
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
	},

	addTweet: function(tweet) {
		var scroll = shouldScrollToBottom();

		var elements = $('div.tweet_' + tweet.id_str)
			.removeClass('tweet_' + tweet.id_str)
			.addClass('tweet');

		tweet.text = markdownToHtml(tweet.text);

		var content = $('#tweet-template').render(tweet);
		elements.append(content);
		//$("time.js-relative-date").timeago();

		$('a', elements).removeAttr('target').attr('target', '_blank');

		pub.scrollToBottom(scroll);
	},

	addGitHubIssue: function (issue) {
		var scroll = shouldScrollToBottom ();

		var elements = $('div.git-hub-issue-' + issue.data.number)
			.removeClass('git-hub-issue-' + issue.data.number);


		issue.data.body = markdownToHtml(encodeHtml(issue.data.body));

		var content = $('#github-issues-template').render(issue.data);
		elements.append(content);

		$('a', elements).removeAttr('target').attr('target', '_blank');

		//$('.js-relative-date').timeago();

		pub.scrollToBottom(scroll);

		elements.append('<script src="https://api.github.com/users/' + issue.data.user.login + '?callback=addGitHubIssuesUser"></script>');
		if (issue.data.assignee != undefined) {
			elements.append('<script src="https://api.github.com/users/' + issue.data.assignee.login + '?callback=addGitHubIssuesUser"></script>');
		}
	}
};

return pub;

}());


$(function() {

	JabbR.initialize();

	window.captureDocumentWrite = JabbR.captureDocumentWrite;

	window.addTweet = JabbR.addTweet;

	window.addGitHubIssue = JabbR.addGitHubIssue;

	window.addGitHubIssuesUser = function (user) {
		var elements = $("a.github-issue-user-" + user.data.login);
		elements.attr("href", user.data.html_url);
	};
});