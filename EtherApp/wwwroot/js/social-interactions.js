// File: EtherApp/wwwroot/js/social-interactions.js

document.addEventListener('DOMContentLoaded', function () {
    // Set up event delegation for all interactive elements
    document.addEventListener('click', function (event) {
        // Post interactions
        if (event.target.closest('.like-button')) {
            event.preventDefault();
            handleFormSubmit(event.target.closest('.like-button'));
        }

        if (event.target.closest('.favorite-button')) {
            event.preventDefault();
            handleFormSubmit(event.target.closest('.favorite-button'));
        }

        // Friend interactions - Already working
        if (event.target.closest('.add-friend-btn')) {
            event.preventDefault();
            handleFriendRequest(event.target.closest('.add-friend-btn'));
        }

        // Friend interactions - Remove friend
        if (event.target.closest('.remove-friend-btn')) {
            event.preventDefault();
            handleFriendAction(event.target.closest('.remove-friend-btn'));
        }

        // Friend interactions - Accept/Reject request
        if (event.target.closest('.friend-request-btn')) {
            event.preventDefault();
            handleFriendAction(event.target.closest('.friend-request-btn'));
        }

        // Delete comment button
        if (event.target.closest('.delete-comment-btn')) {
            event.preventDefault();
            handleCommentDelete(event.target.closest('.delete-comment-btn'));
        }

        // Report post button
        if (event.target.closest('.report-post-btn')) {
            event.preventDefault();
            handleReportPost(event.target.closest('.report-post-btn'));
        }

        // Toggle post visibility button
        if (event.target.closest('.toggle-visibility-btn')) {
            event.preventDefault();
            handleToggleVisibility(event.target.closest('.toggle-visibility-btn'));
        }
    });

    // Handle comment form submissions
    document.addEventListener('submit', function (event) {
        const form = event.target;

        if (form.classList.contains('comment-form')) {
            event.preventDefault();
            handleCommentForm(form);
        }
    });

    // Initialize UI components
    initializeUIComponents();
});

// Initialize UIkit components
function initializeUIComponents() {
    // Initialize any UIkit components
    if (typeof UIkit !== 'undefined') {
        UIkit.util.$$('[uk-dropdown]').forEach(el => {
            UIkit.dropdown(el);
        });

        UIkit.util.$$('[uk-switcher]').forEach(el => {
            UIkit.switcher(el);
        });
    }
}

// Handle likes and favorites
function handleFormSubmit(button) {
    const form = button.closest('form');
    const postId = form.querySelector('input[name="postId"]').value;
    const postContainer = document.getElementById('post-' + postId);

    // Show loading state
    button.classList.add('animate-pulse');

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.text())
        .then(html => {
            // Create a temporary container to parse the HTML
            const tempDiv = document.createElement('div');
            tempDiv.innerHTML = html;

            // Get the updated post element
            const updatedPost = tempDiv.firstElementChild;

            // Replace the current post with the updated one
            postContainer.replaceWith(updatedPost);

            // Reinitialize UI components
            initializeUIComponents();
        })
        .catch(error => {
            console.error('Error: ', error);
            button.classList.remove('animate-pulse');
        });
}

// Handle comment form submissions
function handleCommentForm(form) {
    const postId = form.querySelector('input[name="postId"]').value;
    const postContainer = document.getElementById('post-' + postId);
    const commentInput = form.querySelector('textarea[name="content"]');
    const submitButton = form.querySelector('button[type="submit"]');

    if (!commentInput.value.trim()) {
        return;
    }

    // Disable submit button and show loading
    submitButton.disabled = true;
    submitButton.innerHTML = '<ion-icon name="hourglass-outline" class="animate-spin"></ion-icon>';

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.text())
        .then(html => {
            postContainer.outerHTML = html;
            initializeUIComponents();
        })
        .catch(error => {
            console.error('Error: ', error);
            submitButton.disabled = false;
            submitButton.innerHTML = 'Comment';
        });
}

// Handle friend request (for Suggested Friends component)
function handleFriendRequest(button) {
    const form = button.closest('form');

    // Store the original button text
    const originalHTML = button.innerHTML;

    // Show loading state
    button.disabled = true;
    button.classList.add('opacity-70');
    button.innerHTML = '<ion-icon name="hourglass-outline" class="animate-spin"></ion-icon>';

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Replace button with success message
                button.innerHTML = '<ion-icon name="checkmark-outline"></ion-icon> Sent';
                button.classList.remove('bg-[#4a3256]');
                button.classList.add('bg-green-500');
                button.disabled = true;
            } else {
                // Reset button state
                button.disabled = false;
                button.classList.remove('opacity-70');
                button.innerHTML = originalHTML;
                console.error('Error sending friend request');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            // Reset button state
            button.disabled = false;
            button.classList.remove('opacity-70');
            button.innerHTML = originalHTML;
        });
}

// Handle friend actions (remove, accept/reject requests)
function handleFriendAction(button) {
    const form = button.closest('form');

    // Show loading state
    button.disabled = true;
    button.classList.add('opacity-70');

    // Store the original button text
    const originalHTML = button.innerHTML;
    button.innerHTML = '<ion-icon name="hourglass-outline" class="animate-spin"></ion-icon>';

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Handle different actions by detecting parent container
                const container = findParentContainer(button);

                if (container) {
                    // Fade out and remove container
                    container.style.opacity = '0';
                    container.style.transform = 'scale(0.95)';
                    container.style.transition = 'all 0.3s ease';

                    setTimeout(() => {
                        container.remove();
                    }, 300);
                } else {
                    // If no container found, just reload the page
                    window.location.reload();
                }
            } else {
                // Reset button state
                button.disabled = false;
                button.classList.remove('opacity-70');
                button.innerHTML = originalHTML;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            // Reset button state
            button.disabled = false;
            button.classList.remove('opacity-70');
            button.innerHTML = originalHTML;
        });
}

// Handle comment deletion
function handleCommentDelete(button) {
    const commentId = button.closest('form').querySelector('input[name="commentId"]').value;
    openCommentDeleteDialog(commentId, null);
}

// Handle post reporting
function handleReportPost(button) {
    const form = button.closest('form');

    // Close dropdown if open
    closeDropdown(null, form);

    // Show loading state
    const originalHTML = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<ion-icon name="hourglass-outline" class="animate-spin"></ion-icon> Reporting...';

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Replace button with reported message
                button.innerHTML = '<ion-icon name="checkmark-outline" class="text-xl text-[#4a3256]"></ion-icon><p class="text-[#4a3256]">Reported</p>';
                button.disabled = true;
            } else {
                // Reset button state
                button.disabled = false;
                button.innerHTML = originalHTML;
                alert('Failed to report post.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            // Reset button state
            button.disabled = false;
            button.innerHTML = originalHTML;
        });
}

// Handle post visibility toggle
function handleToggleVisibility(button) {
    const form = button.closest('form');

    // Close dropdown if open
    closeDropdown(null, form);

    // Show loading state
    const originalHTML = button.innerHTML;
    button.disabled = true;
    button.innerHTML = '<ion-icon name="hourglass-outline" class="animate-spin"></ion-icon> Updating...';

    fetch(form.action, {
        method: 'POST',
        body: new FormData(form),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update the button text based on new visibility
                const newPrivacyStatus = data.isPrivate;
                button.innerHTML = newPrivacyStatus ?
                    '<ion-icon name="lock-open-outline" class="text-xl text-[#4a3256]"></ion-icon><p class="text-[#4a3256]"> Set as public</p>' :
                    '<ion-icon name="lock-closed-outline" class="text-xl text-[#4a3256]"></ion-icon><p class="text-[#4a3256]"> Set as private</p>';

                button.disabled = false;
            } else {
                // Reset button state
                button.disabled = false;
                button.innerHTML = originalHTML;
                alert('Failed to update post visibility.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            // Reset button state
            button.disabled = false;
            button.innerHTML = originalHTML;
        });
}

// Function to handle post delete dialog
function openPostDeleteDialog(postId, event) {
    // Reset any existing value
    const input = document.getElementById('deleteDialogPostId');
    if (input) {
        input.value = postId;
        console.log('Setting postId for deletion:', postId);

        // Get the dialog and show it
        const dialog = document.getElementById('postDeleteDialog');
        if (dialog && typeof UIkit !== 'undefined') {
            UIkit.modal('#postDeleteDialog').show();
        } else {
            console.error('Post delete dialog not found or UIkit not loaded');
        }
    } else {
        console.error('deleteDialogPostId input not found in the DOM');
    }

    // Find and hide any active dropdown
    closeDropdown(event);

    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    return false;
}

// Submit the post delete form via AJAX
function submitPostDeleteForm() {
    const form = document.getElementById('postDeleteForm');

    if (!form) {
        console.error('Post delete form not found');
        return;
    }

    const postId = document.getElementById('deleteDialogPostId').value;
    if (!postId) {
        console.error('No postId found in the hidden input');
        return;
    }

    console.log('Attempting to delete post with ID:', postId);

    const postContainer = document.getElementById('post-' + postId);
    if (!postContainer) {
        console.error('Post container not found for ID:', postId);
        return;
    }

    // Show loading state on the post
    postContainer.classList.add('opacity-50');

    // Create a new FormData instance
    const formData = new FormData(form);

    // Log form data for debugging
    for (let pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok: ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log('Delete post response:', data);

            // Hide the modal
            if (typeof UIkit !== 'undefined') {
                UIkit.modal('#postDeleteDialog').hide();
            }

            if (data.success) {
                // Fade out and remove post
                postContainer.style.opacity = '0';
                postContainer.style.transform = 'scale(0.95)';
                postContainer.style.transition = 'all 0.3s ease';

                setTimeout(() => {
                    postContainer.remove();
                }, 300);
            } else {
                // Reset post state
                postContainer.classList.remove('opacity-50');
                alert('Failed to delete post.');
            }
        })
        .catch(error => {
            console.error('Error deleting post:', error);
            postContainer.classList.remove('opacity-50');

            // Hide the modal on error
            if (typeof UIkit !== 'undefined') {
                UIkit.modal('#postDeleteDialog').hide();
            }
        });
}



// Close dropdown menus
function closeDropdown(event, element) {
    let dropdown;

    if (element) {
        dropdown = element.closest('[uk-dropdown]');
    } else if (event) {
        dropdown = event.target.closest('[uk-dropdown]');
    } else {
        const activeDropdown = document.querySelector('.post-options-dropdown');
        if (activeDropdown) {
            dropdown = activeDropdown.closest('[uk-dropdown]');
        }
    }

    if (dropdown && typeof UIkit !== 'undefined') {
        UIkit.dropdown(dropdown).hide(true);
    }
}

// Find the appropriate parent container for removal
function findParentContainer(element) {
    // For friend list items
    const friendItem = element.closest('.flex.flex-col.items-center');
    if (friendItem) return friendItem;

    // For friend requests in Friends/Index.cshtml
    const requestItem = element.closest('.flex.items-center.gap-3');
    if (requestItem) return requestItem;

    return null;
}

// Function to handle comment delete dialog
function openCommentDeleteDialog(commentId, event) {
    // Reset any existing value
    const input = document.getElementById('deleteDialogCommentId');
    if (input) {
        input.value = commentId;
        console.log('Setting commentId for deletion:', commentId);

        // Get the dialog and show it
        const dialog = document.getElementById('commentDeleteDialog');
        if (dialog && typeof UIkit !== 'undefined') {
            UIkit.modal('#commentDeleteDialog').show();
        } else {
            console.error('Comment delete dialog not found or UIkit not loaded');
        }
    } else {
        console.error('deleteDialogCommentId input not found in the DOM');
    }

    if (event) {
        event.preventDefault();
        event.stopPropagation();
    }
    return false;
}

// Submit the comment delete form via AJAX
function submitCommentDeleteForm() {
    const form = document.getElementById('commentDeleteForm');

    if (!form) {
        console.error('Comment delete form not found');
        return;
    }

    const commentId = document.getElementById('deleteDialogCommentId').value;
    if (!commentId) {
        console.error('No commentId found in the hidden input');
        return;
    }

    console.log('Attempting to delete comment with ID:', commentId);

    // Use a more reliable way to find the comment
    let commentItem = null;

    // First try the usual selector
    const selector = `.flex.items-start.gap-3 button[onclick*="${commentId}"]`;
    const deleteButton = document.querySelector(selector);
    if (deleteButton) {
        commentItem = deleteButton.closest('.flex.items-start.gap-3');
    }

    // If not found, try a more generic approach
    if (!commentItem) {
        // Look for the comment by ID directly in comment sections
        document.querySelectorAll('.flex.items-start.gap-3').forEach(item => {
            if (item.innerHTML.includes(commentId)) {
                commentItem = item;
            }
        });
    }

    if (!commentItem) {
        console.error('Comment container not found for ID:', commentId);
        // Still proceed with the delete request, but we won't be able to animate the removal
    }

    // Show loading state on the comment if found
    if (commentItem) {
        commentItem.classList.add('opacity-50');
    }

    // Create a new FormData instance
    const formData = new FormData(form);

    // Log form data for debugging
    for (let pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }

    fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok: ' + response.status);
            }
            return response.json();
        })
        .then(data => {
            console.log('Delete comment response:', data);

            // Hide the modal
            if (typeof UIkit !== 'undefined') {
                UIkit.modal('#commentDeleteDialog').hide();
            }

            if (data.success) {
                if (commentItem) {
                    // Fade out and remove comment
                    commentItem.style.opacity = '0';
                    commentItem.style.transform = 'scale(0.95)';
                    commentItem.style.transition = 'all 0.3s ease';

                    setTimeout(() => {
                        commentItem.remove();

                        // Find the post container and update comment count
                        const postContainers = document.querySelectorAll('[id^="post-"]');
                        postContainers.forEach(container => {
                            const commentCountElement = container.querySelector('.flex.items-center.gap-2:nth-child(2) a');
                            if (commentCountElement) {
                                const currentCount = parseInt(commentCountElement.textContent.trim());
                                if (!isNaN(currentCount) && currentCount > 0) {
                                    commentCountElement.textContent = (currentCount - 1).toString();
                                }
                            }
                        });
                    }, 300);
                } else {
                    // If we couldn't find the comment element, just reload the page
                    window.location.reload();
                }
            } else {
                // Reset comment state if found
                if (commentItem) {
                    commentItem.classList.remove('opacity-50');
                }
                alert('Failed to delete comment.');
            }
        })
        .catch(error => {
            console.error('Error deleting comment:', error);

            // Reset comment state if found
            if (commentItem) {
                commentItem.classList.remove('opacity-50');
            }

            // Hide the modal on error
            if (typeof UIkit !== 'undefined') {
                UIkit.modal('#commentDeleteDialog').hide();
            }
        });
}