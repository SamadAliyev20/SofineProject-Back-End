$(document).ready(function () {
    $(document).on('click', '.decrease-count', (function () {
        let productid = $(this).attr('data-id')
        fetch("/basket/DecreaseBasket?productId=" + productid)
            .then(res => {
                return res.text();
            })
            .then(data => {
                $('.cart-section').html(data)
                fetch("/basket/GetBasketForMiniCart")
                    .then(res => {
                        return res.text();
                    })
                    .then(data => {
                        $('.mini-cart-inner-content').html(data)
                    })
            })
    }))
    $(document).on('click', '.increase-count', (function () {
        let productid = $(this).attr('data-id')
        fetch("/basket/IncreaseBasket?productId=" + productid)
            .then(res => {
                return res.text();
            })
            .then(data => {
                $('.cart-section').html(data)
                fetch("/basket/GetBasketForMiniCart")
                    .then(res => {
                        return res.text();
                    })
                    .then(data => {
                        $('.mini-cart-inner-content').html(data)
                    })
            })
    }))
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
                    title: 'Product Added to Cart'
                })
                fetch('basket/GetBasketCount')
                    .then(res => {
                        return res.json();
                    }).then(data => {
                        $('.count').text(data);
                    });
               
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
                    title: 'Product Removed from Cart',
                    customClass: {
                        container: 'my-sweet-alert'
                    }
                })
                fetch('/Basket/GetBasketForBasket/').then(res => {
                    return res.text();
                }).then(data => {
                    $('.cart-section').html(data)
                })
                fetch('/basket/GetBasketCount')
                    .then(res => {
                        return res.json();
                    }).then(data => {
                        $('.count').text(data);
                    });
               
            }
        })
    })
    $(".addToCard").on('click', function (e) {
        e.preventDefault();
        let productId = $(this).attr('data-id');

        fetch('/basket/AddBasket?id=' + productId)
            .then(res => {
                return res.text();
            })
            .then(data => {
                $('.mini-cart-inner-content').html(data);
                fetch('/basket/GetBasketCount')
                    .then(res => {
                        return res.json();
                    }).then(data => {
                        $('.count').text(data);
                    });
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
                    title: 'Product Added to Cart'
                })
               
            });

    });
    $(".addToWishlist").on('click', function (e) {
        e.preventDefault();
        let productId = $(this).attr('data-id');
        fetch('/Wishlist/AddWishlist?id=' + productId)
            .then(res => {
                return res.text();
            }).then(data => {
                console.log(data)
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
                    title: 'Product Added to WishList'
                })
                fetch('/Wishlist/GetWishlistCount')
                    .then(res => {
                        return res.json();
                    }).then(data => {
                        $('.wish-count').text(data);
                    });
            })
    })
    $(document).on('click', '.delete-wishList', function (e) {
        e.preventDefault();
        let productId = $(this).attr('data-id');
        fetch('/Wishlist/DeleteWishList/' + productId).then(res => {
            return res.text();
        }).then(data => {
            if (data != null) {
                $('.wishlist-inner-content').html(data)
                fetch('/Wishlist/GetWishlistForCart/').then(res => {
                    return res.text();
                }).then(data => {
                    $('.wish-area').html(data)
                })
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
                    title: 'Product Removed from WishList'
                })
                fetch('/Wishlist/GetWishlistCount')
                    .then(res => {
                        return res.json();
                    }).then(data => {
                        $('.wish-count').text(data);
                    });
            }
           
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

        fetch('/Shop/RangeFilter?range=' + value )
            .then(res => {
                return res.text();
            }).then(data => {
                $('.pro-area').html(data)
                $(".addToBasket").on('click', function (e) {
                    e.preventDefault();
                    let productId = $(this).data('id');

                    fetch('/basket/AddBasket?id=' + productId)
                        .then(res => {
                            return res.text();
                        })
                        .then(data => {

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
                                title: 'Product Added to Cart'
                            })
                            fetch('basket/GetBasketCount')
                                .then(res => {
                                    return res.json();
                                }).then(data => {
                                    $('.count').text(data);
                                });
                        })
                })
                $(".productQuickModal").on('click', function (e) {
                    e.preventDefault();
                    let url = $(this).attr('href');
                    fetch(url).then(res => {
                        return res.text();
                    })
                        .then(data => {
                            $('.modal-content').html(data);
                            $('.modal').modal('show');
                            $('.modal').on('shown.bs.modal', function (e) {
                                e.preventDefault();
                                $('.product-modal-carousel').slick('setPosition');
                                $(".addToCard").on('click', function (e) {
                                    e.preventDefault();
                                    let productId = $(this).attr('data-id');

                                    fetch('/basket/AddBasket?id=' + productId)
                                        .then(res => {
                                            return res.text();
                                        })
                                        .then(data => {
                                            $('.mini-cart-inner-content').html(data);
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
                                                title: 'Product Added to Cart'
                                            })
                                            fetch('basket/GetBasketCount')
                                                .then(res => {
                                                    return res.json();
                                                }).then(data => {
                                                    $('.count').text(data);
                                                });
                                        });

                                });
                                $(".addToWishlist").on('click', function (e) {
                                    e.preventDefault();
                                    let productId = $(this).attr('data-id');
                                    fetch('/Wishlist/AddWishlist?id=' + productId)
                                        .then(res => {
                                            return res.text();
                                        })
                                        .then(data => {
                                            console.log(data)
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
                                                title: 'Product Added to WishList'
                                            })
                                            fetch('/Wishlist/GetWishlistCount')
                                                .then(res => {
                                                    return res.json();
                                                }).then(data => {
                                                    $('.wish-count').text(data);
                                                });
                                        })
                                })

                            })
                            $('.product-modal-carousel').slick({
                                infinite: false,
                                speed: 600,
                                slidesToShow: 1,
                                slidesToScroll: 1,
                                arrows: false,
                                swipe: true,
                                fade: true,
                                dots: true,
                                responsive: [
                                    {
                                        breakpoint: 1024,
                                        settings: {
                                            slidesToShow: 1,
                                            slidesToScroll: 1,
                                            infinite: false,
                                            swipe: true,
                                            arrows: false,
                                        }
                                    },
                                    {
                                        breakpoint: 1400,
                                        settings: {
                                            slidesToShow: 1,
                                            slidesToScroll: 1,
                                            infinite: false,
                                        }
                                    },
                                    {
                                        breakpoint: 600,
                                        settings: {
                                            slidesToShow: 1,
                                            arrows: false,
                                            slidesToScroll: 1,
                                            swipe: true
                                        }
                                    },
                                    {
                                        breakpoint: 480,
                                        settings: {
                                            slidesToShow: 1,
                                            arrows: false,
                                            slidesToScroll: 1,
                                            swipe: true
                                        }
                                    }
                                ]
                            });

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
                                title: 'Product Added to WishList'
                            })
                            fetch('/Wishlist/GetWishlistCount')
                                .then(res => {
                                    return res.json();
                                }).then(data => {
                                    $('.wish-count').text(data);
                                });
                        })
                })
            })
    })
    $(document).on('click', '.addAddress', function (e) {
        e.preventDefault();
        $('.addressContainer').addClass('d-none');
        $('.addressForm').removeClass('d-none');
    });
    $('.accordion-collapse').on('show.bs.collapse', function () {
        $(this).closest("table")
            .find(".accordion-collapse.show")
            .not(this)
            .collapse('toggle');
    })
    $('.addToWishlist i').on('click', function () {
        $(this).removeClass('fa-regular fa-heart').addClass("fa-solid fa-heart")
    })
   
})

