$(document).ready(function () {
    $(".productQuickModal").on('click', function () {
        
        let url = $(this).attr('href');
        fetch(url).then(res => {
            return res.text();
        })
            .then(data => {
                $('.modal-content').html(data);
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
})