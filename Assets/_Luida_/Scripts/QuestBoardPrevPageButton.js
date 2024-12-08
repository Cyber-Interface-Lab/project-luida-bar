$.onInteract(() => {
    $.getItemsNear($.getPosition(), 0.1).forEach(item => {
        item.send("quest_board_to_prev", true);
    });
})