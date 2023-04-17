$(document).ready(function () {
    // product quantity
    $('.add').click(function () {
        if ($(this).prev().val()) {
            $(this).prev().val(+$(this).prev().val() + 1);
        }
    });
    $('.sub').click(function () {
        if ($(this).next().val() > 1) {
            if ($(this).next().val() > 1) $(this).next().val(+$(this).next().val() - 1);
        }
    });
    $('.searchInput').on('keyup', function () {
        let search = $(this).val();
        if (search.trim().length >= 3 && search != null) {
            fetch('product/search?search=' + search)
                .then(res => {
                    return res.text();
                }).then(data => {
                    $('.search-body').html(data);
                })
        }
        else {
            $('.search-body').html("");
        }	
    })
    $(".addToBasket").on('click', function (e) {
        e.preventDefault();
        let productId = $(this).data('id');

        fetch('/basket/AddBasket?id=' + productId)
            .then(res => {
                return res.text();
            }).then(data => {
                $('.mini-cart-inner-content').html(data);
            });

    });
    $(document).on('click', '.product-delete', function (e) {
        e.preventDefault();
        let productId = $(this).attr('data-productId');
        fetch('/Basket/DeleteBasket/' + productId).then(res => {
            return res.text();
        }).then(data => {
            if (data != null) {
                $('.mini-cart-inner-content').html(data)
                fetch('/Basket/GetBasketForBasket/').then(res => {
                    return res.text();
                }).then(data => {
                    $('.cart-section').html(data)
                })
            }
        })
    })
})