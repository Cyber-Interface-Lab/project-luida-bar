$.onInteract(() => {
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        item.send("form_to_next", true);
    });
})