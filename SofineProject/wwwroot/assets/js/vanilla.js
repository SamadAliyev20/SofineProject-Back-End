$(document).ready(function () {
    $(".productQuickModal").on('click', function (e) {
        e.preventDefault();
        let url = $(this).attr('href');
        fetch(url).then(res => {
            return res.text();
        })
            .then(data => {
                $('.modal-content').html(data);
                $('.modal').on('shown.bs.modal', function (e) {
                    e.preventDefault();
                    $('.product-modal-carousel').slick('setPosition');
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
})