$(function() {
    
    "use strict";
    
    //===== Prealoder
    
    $(window).on('load', function(event) {
        $('#preloader').delay(500).fadeOut(500);
    });
    
    
    //===== Sticky
    
    $(window).on('scroll', function(event) {    
        var scroll = $(window).scrollTop();
        if (scroll < 20) {
            $(".navbar-area").removeClass("sticky");
        } else{
            $(".navbar-area").addClass("sticky");
        }
    });
    
    
    //===== Section Menu Active

    var scrollLink = $('.page-scroll');
        // Active link switching
        $(window).scroll(function() {
        var scrollbarLocation = $(this).scrollTop();

        scrollLink.each(function() {

          var sectionOffset = $(this.hash).offset().top - 73;

          if ( sectionOffset <= scrollbarLocation ) {
            $(this).parent().addClass('active');
            $(this).parent().siblings().removeClass('active');
          }
        });
    });
    
    
    //===== close navbar-collapse when a  clicked
    
    $(".navbar-nav a").on('click', function () {
        $(".navbar-collapse").removeClass("show");
    });
    
    
    //===== Mobile Menu
    
    $(".navbar-toggler").on('click', function(){
        $(this).toggleClass("active");
    });
    
    $(".navbar-nav a").on('click', function() {
        $(".navbar-toggler").removeClass('active');
    });
    
    var subMenu = $(".sub-menu-bar .navbar-nav .sub-menu");
    
    if(subMenu.length) {
        subMenu.parent('li').children('a').append(function () {
            return '<button class="sub-nav-toggler"> <i class="lni-chevron-down"></i> </button>';
        });
        
        var subMenuToggler = $(".sub-menu-bar .navbar-nav .sub-nav-toggler");
        
        subMenuToggler.on('click', function() {
            $(this).parent().parent().children(".sub-menu").slideToggle();
            return false
        });
        
    }
    
    
    //===== Isotope Project 1
    
    $('.container').imagesLoaded(function () {
        var $grid = $('.grid').isotope({
        // options
            transitionDuration: '1s'
        });
        
        // filter items on button click
        $('.portfolio-menu ul').on( 'click', 'li', function() {
          var filterValue = $(this).attr('data-filter');
          $grid.isotope({ filter: filterValue });
        });
        
        //for menu active class
        $('.portfolio-menu ul li').on('click', function (event) {
            $(this).siblings('.active').removeClass('active');
            $(this).addClass('active');
            event.preventDefault();
        });
    });
    
    
    //  Magnific Popup
    
    $('.image-popup').magnificPopup({
      type: 'image',
      gallery:{
        enabled:true
      }
    });

    
    //===== Slick testimonials
    
    $('.testimonial-active').slick({
        infinite: true,
        slidesToShow: 2,
        slidesToScroll: 1,
        speed: 800,
        arrows: false,
        dots: true,
        autoplay: true,
        autoplaySpeed: 5000,
        responsive: [
            {
              breakpoint: 1200,
              settings: {
                slidesToShow: 2,
              }
            },
            {
              breakpoint: 992,
              settings: {
                slidesToShow: 2,
              }
            },
            {
              breakpoint: 768,
              settings: {
                slidesToShow: 1,
              }
            },
            {
              breakpoint: 576,
              settings: {
                slidesToShow: 1,
                arrows: false,
              }
            }
        ]
    });
    
    
    //===== Back to top
    
    // Show or hide the sticky footer button
    $(window).on('scroll', function(event) {
        if($(this).scrollTop() > 900){
            $('.back-to-top').fadeIn(200)
        } else{
            $('.back-to-top').fadeOut(200)
        }
    });
    
    
    //Animate the scroll to yop
    $('.back-to-top').on('click', function(event) {
        event.preventDefault();
        
        $('html, body').animate({
            scrollTop: 0,
        }, 1500);
    });
    
    
    //===== 
       //===== Slick userlogos
    
    $('.userlogos-active').slick({
        infinite: true,
        speed: 800,
        arrows: true,
        nextArrow: '<i class="lni lni-chevron-right slick-next"></i>',
        prevArrow: '<i class="lni lni-chevron-left slick-prev"></i>',
        dots: false,
        initialSlide: Math.floor(Math.random() * 20),
        slidesToShow: 6,
        slidesToScroll: 1,
        responsive: [
            {
              breakpoint: 1200,
              settings: {
                slidesToShow: 4,
              }
            },
            {
              breakpoint: 992,
              settings: {
                slidesToShow: 3,
              }
            },
            {
              breakpoint: 768,
              settings: {
                slidesToShow: 2,
              }
            },
            {
              breakpoint: 576,
              settings: {
                slidesToShow: 1,
                arrows: false,
                centerMode: true,
              }
            }
        ]
    });
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
});