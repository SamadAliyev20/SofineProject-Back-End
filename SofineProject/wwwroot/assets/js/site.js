$(document).ready(function () {
  $('.modal').on('shown.bs.modal', function (e) {
    e.preventDefault();
    $('.product-modal-carousel').slick('setPosition');
  })
  $('.hero-slider').slick({
    dots: true,
    infinite: false,
    arrows: false,
    speed: 600,
    slidesToShow: 1,
    slidesToScroll: 1,
    swipe: true,
    responsive: [
      {
        breakpoint: 1024,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          infinite: true,
          dots: true
        }
      },
      {
        breakpoint: 600,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          swipe: true
        }
      },
      {
        breakpoint: 480,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          swipe: true
        }
      }
    ]
  });
  $('.product-slider').slick({
    infinite: false,
    speed: 600,
    arrows: false,
    slidesToShow: 5,
    slidesToScroll: 1,
    swipe: true,
    responsive: [
      {
        breakpoint: 1024,
        settings: {
          slidesToShow: 2,
          slidesToScroll: 1,
          infinite: true,

        }
      },
      {
        breakpoint: 599,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          infinite: true,

        }
      },
      {
        breakpoint: 767,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          infinite: true,

        }
      },
      {
        breakpoint: 1649,
        settings: {
          slidesToShow: 3,
          slidesToScroll: 1,
          infinite: true,

        }
      },
      {
        breakpoint: 600,
        settings: {
          slidesToShow: 2,
          slidesToScroll: 1,
          swipe: true
        }
      },
      {
        breakpoint: 480,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          swipe: true
        }
      }
    ]
  });
  $('.category-carousel').slick({
    infinite: false,
    speed: 600,
    slidesToShow: 3,
    slidesToScroll: 1,
    arrows: true,
    swipe: true,
    responsive: [
      {
        breakpoint: 1024,
        settings: {
          slidesToShow: 1,
          slidesToScroll: 1,
          infinite: false,
          arrows: false,
        }
      },
      {
        breakpoint: 1400,
        settings: {
          slidesToShow: 2,
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
  $('.brand-carousel').slick({
    infinite: false,
    slidesToShow: 6,
    speed: 600,
    arrows: false,
    swipe: false,
    settings: "unslick",
    responsive: [
      {
        breakpoint: 1024,
        settings: {
          slidesToShow: 3,
          slidesToScroll: 1,
          infinite: false,
          swipe: true,
          arrows: false,
        }
      },
      {
        breakpoint: 1400,
        settings: {
          slidesToShow: 4,
          slidesToScroll: 1,
          infinite: false,
        }
      },
      {
        breakpoint: 600,
        settings: {
          slidesToShow: 2,
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
  $('.hamburger-btn').on('click', function (e) {
    e.preventDefault();
    $('.mobile-menu-content-box').toggleClass('active')
    $('.mask_opened').toggleClass('active')
  });
  $('.mobile-close-btn').on('click', function (e) {
    e.preventDefault();
    $('.mobile-menu-content-box').toggleClass('active');
    $('.mask_opened').toggleClass('active')
  });
  ///cart-open-close-section///
  $('.minicart-btn').on('click', function (e) {
    e.preventDefault();
    $('.mini-cart-box').toggleClass('active');
    $('.mask_opened').toggleClass('active')
    //   document.body.style.overflow = "hidden"; // ADD THIS LINE
    // document.body.style.height = "100%"; // ADD THIS LINE
  });
  $('.cart-close-btn').on('click', function (e) {
    e.preventDefault();
    $('.mini-cart-box').toggleClass('active');
    $('.mask_opened').toggleClass('active')
  });
  ///cart-open-close-section///
  ///Wishlist-open-close-section///
  $('.miniwishlist-btn').on('click', function (e) {
    e.preventDefault();
    $('.wishlist-box').toggleClass('active');
    $('.mask_opened').toggleClass('active');
  });
  $('.wislist-close-btn').on('click', function () {
    $('.wishlist-box').toggleClass('active');
    $('.mask_opened').toggleClass('active')
  });
  ///Wishlist-open-close-section///
  ///Search-open-close-section///
  $('.search-btn').on('click', function (e) {
    e.preventDefault();
    $('.search-content-box').toggleClass('active');
  });
  $('.search-close-btn').on('click', function () {
    $('.search-content-box').toggleClass('active');
  });
  ///Search-open-close-section///
  // pricing filter
  var rangeSlider = $(".price-range"),
    amount = $(".filter-amount"),
    minPrice = rangeSlider.data('min'),
    maxPrice = rangeSlider.data('max');
  rangeSlider.slider({
    range: true,
    min: minPrice,
    max: maxPrice,
    values: [minPrice, maxPrice],
    slide: function (event, ui) {
      amount.val("$" + ui.values[0] + " - $" + ui.values[1]);
    }
  });
  amount.val(" $" + rangeSlider.slider("values", 0) +
    " - $" + rangeSlider.slider("values", 1));
  // pricing filter
  // nice select
  $('select').niceSelect();
  // nice-select
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
  

});