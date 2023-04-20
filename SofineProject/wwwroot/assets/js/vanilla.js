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

        fetch('basket/AddBasket?id=' + productId)
            .then(res => {
                return res.text();
            }).then(data => {

                $('.mini-cart-inner-content').html(data)
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-end',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: false,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })

                Toast.fire({
                    icon: 'success',
                    title: 'Məhsul Səbətə Əlavə Olundu'
                })
            })
    })
    $(document).on('click', '.product-delete', function (e) {
        e.preventDefault();
        let productId = $(this).attr('data-productId');
        fetch('/Basket/DeleteBasket/' + productId).then(res => {
            return res.text();
        }).then(data => {
            if (data != null) {
                $('.mini-cart-inner-content').html(data)
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-start',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: false,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })

                Toast.fire({
                    icon: 'success',
                    title: 'Məhsul Səbətdən Silindi',
                    customClass: {
                        container: 'my-sweet-alert'
                    }
                })
                fetch('/Basket/GetBasketForBasket/').then(res => {
                    return res.text();
                }).then(data => {
                    $('.cart-section').html(data)
                })
            }
        })
    })
    $(".addToWishlist").on('click', function (e) {
        e.preventDefault();

        let productId = $(this).data('id');
        fetch('Wishlist/AddWishlist?id=' + productId)
            .then(res => {
                return res.text();
            }).then(data => {
                $('.wishlist-inner-content').html(data)
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-end',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: false,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })

                Toast.fire({
                    icon: 'success',
                    title: 'Məhsul İstəklərə Əlavə Olundu'
                })
            })
    })
    $(document).on('click', '.delete-wishList', function (e) {
        e.preventDefault();
        let productId = $(this).attr('data-productId');
        fetch('Wishlist/DeleteWishList/' + productId).then(res => {
            return res.text();
        }).then(data => {
            if (data != null) {
                $('.wishlist-inner-content').html(data)
                const Toast = Swal.mixin({
                    toast: true,
                    position: 'bottom-start',
                    showConfirmButton: false,
                    timer: 3000,
                    timerProgressBar: false,
                    didOpen: (toast) => {
                        toast.addEventListener('mouseenter', Swal.stopTimer)
                        toast.addEventListener('mouseleave', Swal.resumeTimer)
                    }
                })

                Toast.fire({
                    icon: 'success',
                    title: 'Məhsul İstəklərdən silindi'
                })
            }
        })
    })
    $('.category-selector').on('click', function () {
        let categoryID = $(this).data('id')
        let productTypeId = $(this).data('productTypeId')
       
        fetch('/Shop/ProductFilter?categoryId=' + categoryID + '&productTypeId=' + productTypeId)
            .then(res => {
                return res.text();          
            }).then(data => {
                $('.shop-main-area').html(data)
            })
    })
    $('.productType-selector').on('click', function () {
        let categoryId = $(this).attr('data-categoryId');
        let productTypeId = $(this).data('id');
        
        fetch('/Shop/ProductFilter?categoryId=' + categoryId + '&productTypeId=' + productTypeId)
            .then(res => {
                return res.text();
            }).then(data => {
                $('.shop-main-area').html(data);
            })
    });
    $('.sort-select').on('change', function () {
        let sort = $(this).find(':selected').attr('data-sortId')
        fetch('/Shop/ProductFilter?sortId='+sort)
        then(res => {
            return res.text();
        }).then(data => {
            $('.shop-main-area').html(data);
        })
    })
    // pricing filter
    var rangeslider = $(".price-range"),
        amount = $("#amount"),
        minprice = rangeslider.data('min'),
        maxprice = rangeslider.data('max');
    rangeslider.slider({
        range: true,
        min: minprice,
        max: maxprice,
        values: [minprice, maxprice],
        slide: function (event, ui) {
            amount.val("$" + ui.values[0] + " - $" + ui.values[1]);
        }
    });
    amount.val(" $" + rangeslider.slider("values", 0) +
        " - $" + rangeslider.slider("values", 1));
    // pricing filter

    $('.filterInput').on('click', function (e) {
        e.preventDefault();
        let value = $('.rangeInput').val();

        fetch('Shop/ProductFilter?range=' + value)
            .then(res => {
                return res.text();
            }).then(data => {
                $('.shop-main-area').html(data)
            })

    })
 
})

