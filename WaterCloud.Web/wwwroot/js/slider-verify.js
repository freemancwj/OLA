(function ($) {
    $.sliderVerify = {
        create: function (containerId, options) {
            var defaults = {
                width: 300,
                height: 96,
                successText: '验证通过',
                failText: '向右拖动滑块完成拼图',
                onSuccess: function () { },
                onFail: function () { }
            };

            var settings = $.extend({}, defaults, options);
            var container = $('#' + containerId);
            if (!container.length) {
                return null;
            }
            var namespace = '.sliderVerify' + new Date().getTime();

            var html = '<div class="slider-captcha">' +
                '<div class="slider-captcha-img">' +
                '<img class="slider-bg" alt="">' +
                '<img class="slider-piece" alt="">' +
                '<div class="slider-captcha-mask"><i class="layui-icon layui-icon-loading layui-anim layui-anim-rotate layui-anim-loop"></i><span>加载中...</span></div>' +
                '<div class="slider-captcha-refresh" title="换一张"><i class="layui-icon layui-icon-refresh-3"></i></div>' +
                '</div>' +
                '<div class="slider-track">' +
                '<div class="slider-progress"></div>' +
                '<div class="slider-text">' + settings.failText + '</div>' +
                '<div class="slider-btn"><i class="layui-icon layui-icon-right"></i></div>' +
                '</div>' +
                '</div>';

            container.empty().append(html);

            var $captcha = container.find('.slider-captcha');
            var $imgBox = container.find('.slider-captcha-img');
            var $bg = container.find('.slider-bg');
            var $piece = container.find('.slider-piece');
            var $mask = container.find('.slider-captcha-mask');
            var $refresh = container.find('.slider-captcha-refresh');
            var $track = container.find('.slider-track');
            var $progress = container.find('.slider-progress');
            var $text = container.find('.slider-text');
            var $btn = container.find('.slider-btn');
            var $btnIcon = $btn.find('i');

            var token = '';
            var verifyToken = '';
            var naturalWidth = settings.width;
            var naturalHeight = settings.height;
            var pieceBoxSize = 60;
            var pieceTop = 0;
            var pieceLeft = 0;
            var maxLeft = 0;
            var scale = 1;
            var dragging = false;
            var verified = false;
            var verifying = false;
            var loading = false;
            var startClientX = 0;
            var startPieceLeft = 0;

            $captcha.css('width', settings.width + 'px');
            $captcha.find('.slider-captcha-img').css('height', settings.height + 'px');

            function getScale() {
                var boxWidth = $imgBox.width();
                var boxHeight = $imgBox.height();
                // 图片未渲染时按原始宽度计算
                if (boxWidth <= 0 || naturalWidth <= 0) {
                    return 1;
                }
                return Math.min(boxWidth / naturalWidth, naturalHeight > 0 && boxHeight > 0 ? boxHeight / naturalHeight : 1);
            }

            function drawPiece(useTransition) {
                // 拼图按背景图实际渲染位置对齐：横、纵分别按背景图渲染尺寸缩放
                var bgEl = $bg.get(0);
                var bgLeft = bgEl ? bgEl.offsetLeft : 0;
                var bgTop = bgEl ? bgEl.offsetTop : 0;
                var bgW = bgEl ? bgEl.offsetWidth : 0;
                var bgH = bgEl ? bgEl.offsetHeight : 0;
                // 图片未渲染时按原始尺寸计算
                var xScale = (bgW > 0 && naturalWidth > 0) ? bgW / naturalWidth : 1;
                var yScale = (bgH > 0 && naturalHeight > 0) ? bgH / naturalHeight : 1;
                scale = xScale;
                if (useTransition) {
                    $piece.css('transition', 'left 0.2s ease');
                } else {
                    $piece.css('transition', 'none');
                }
                $piece.css({
                    left: (bgLeft + pieceLeft * xScale) + 'px',
                    top: (bgTop + pieceTop * yScale) + 'px',
                    width: (pieceBoxSize * xScale) + 'px',
                    height: (pieceBoxSize * yScale) + 'px'
                });
                var trackWidth = $track.width();
                var btnWidth = $btn.outerWidth() || 42;
                var btnLeft = maxLeft > 0 ? (pieceLeft / maxLeft) * (trackWidth - btnWidth) : 0;
                btnLeft = Math.max(0, Math.min(btnLeft, trackWidth - btnWidth));
                if (useTransition) {
                    $btn.css('transition', 'left 0.2s ease');
                    $progress.css('transition', 'width 0.2s ease');
                } else {
                    $btn.css('transition', 'none');
                    $progress.css('transition', 'none');
                }
                $btn.css('left', btnLeft + 'px');
                $progress.css('width', (btnLeft + btnWidth) + 'px');
            }

            function setLoading(isLoading) {
                loading = isLoading;
                if (isLoading) {
                    $mask.show();
                } else {
                    $mask.hide();
                }
            }

            function setNormalStyle() {
                $track.removeClass('slider-success slider-fail');
                $text.text(settings.failText);
                $btnIcon.removeClass('layui-icon-ok').addClass('layui-icon-right');
            }

            function setFailStyle(message) {
                $track.removeClass('slider-success slider-fail');
                $track.addClass('slider-fail');
                $text.text(message || settings.failText);
                $btnIcon.removeClass('layui-icon-ok').addClass('layui-icon-right');
            }

            function setSuccessStyle() {
                $track.removeClass('slider-fail');
                $track.addClass('slider-success');
                $text.text(settings.successText);
                $btnIcon.removeClass('layui-icon-right').addClass('layui-icon-ok');
            }

            function resetPosition(useTransition) {
                pieceLeft = 0;
                drawPiece(useTransition !== false);
            }

            // 获取一张新的拼图验证码
            function loadCaptcha() {
                if (loading) {
                    return;
                }
                verified = false;
                verifyToken = '';
                token = '';
                setLoading(true);
                setNormalStyle();
                resetPosition(false);
                $.ajax({
                    url: '/Login/GetSliderCaptcha',
                    type: 'get',
                    dataType: 'json',
                    cache: false,
                    success: function (data) {
                        if (!data || data.state !== 'success' || !data.token) {
                            setLoading(false);
                            setFailStyle((data && data.message) || '验证码加载失败，请点击刷新');
                            return;
                        }
                        token = data.token;
                        naturalWidth = data.width || settings.width;
                        naturalHeight = data.height || settings.height;
                        pieceBoxSize = data.pieceBoxSize || 60;
                        pieceTop = data.pieceY || 0;
                        pieceLeft = 0;
                        maxLeft = Math.max(0, naturalWidth - pieceBoxSize);
                        $bg.one('load', function () {
                            setLoading(false);
                            drawPiece(false);
                        });
                        $piece.attr('src', data.pieceImage);
                        $bg.attr('src', data.bgImage);
                        // 图片已缓存时 load 事件可能不再触发
                        if ($bg.get(0) && $bg.get(0).complete) {
                            setLoading(false);
                            drawPiece(false);
                        }
                    },
                    error: function () {
                        setLoading(false);
                        setFailStyle('验证码加载失败，请点击刷新');
                    }
                });
            }

            function startDrag(clientX) {
                if (verified || verifying || loading) {
                    return;
                }
                dragging = true;
                startClientX = clientX;
                startPieceLeft = pieceLeft;
                $track.removeClass('slider-fail');
                $btn.css('transition', 'none');
                $progress.css('transition', 'none');
                $piece.css('transition', 'none');
            }

            function moveDrag(clientX) {
                if (!dragging) {
                    return;
                }
                scale = getScale();
                var moveX = (clientX - startClientX) / scale;
                pieceLeft = Math.max(0, Math.min(maxLeft, startPieceLeft + moveX));
                drawPiece(false);
            }

            function endDrag() {
                if (!dragging) {
                    return;
                }
                dragging = false;
                if (verified || verifying || loading) {
                    return;
                }
                // 位移过小视为误触，直接复位
                if (pieceLeft < 5) {
                    resetPosition(true);
                    return;
                }
                verifying = true;
                $.ajax({
                    url: '/Login/VerifySlider',
                    type: 'post',
                    dataType: 'json',
                    data: { token: token, sliderX: Math.round(pieceLeft) },
                    success: function (data) {
                        verifying = false;
                        if (data && data.state === 'success' && data.verifyToken) {
                            verified = true;
                            verifyToken = data.verifyToken;
                            setSuccessStyle();
                            settings.onSuccess(verifyToken);
                        } else {
                            var message = (data && data.message) || '拼图未对齐，验证失败';
                            setFailStyle(message);
                            resetPosition(true);
                            settings.onFail();
                            // 验证码已失效，稍后自动刷新
                            setTimeout(function () {
                                if (!verified) {
                                    loadCaptcha();
                                }
                            }, 800);
                        }
                    },
                    error: function () {
                        verifying = false;
                        setFailStyle('验证失败，请刷新重试');
                        resetPosition(true);
                        settings.onFail();
                        setTimeout(function () {
                            if (!verified) {
                                loadCaptcha();
                            }
                        }, 800);
                    }
                });
            }

            $btn.on('mousedown' + namespace + ' touchstart' + namespace, function (e) {
                e.preventDefault();
                var clientX = e.type === 'touchstart' ? e.originalEvent.touches[0].clientX : e.clientX;
                startDrag(clientX);
            });

            $(document)
                .on('mousemove' + namespace + ' touchmove' + namespace, function (e) {
                    if (!dragging) {
                        return;
                    }
                    e.preventDefault();
                    var clientX = e.type === 'touchmove' ? e.originalEvent.touches[0].clientX : e.clientX;
                    moveDrag(clientX);
                })
                .on('mouseup' + namespace + ' touchend' + namespace + ' touchcancel' + namespace, function () {
                    endDrag();
                });

            $refresh.on('click', function () {
                if (!verifying) {
                    loadCaptcha();
                }
            });

            // 页面宽度变化时按比例重新绘制
            $(window).on('resize' + namespace, function () {
                drawPiece(false);
            });

            loadCaptcha();

            return {
                reset: function () {
                    loadCaptcha();
                },
                refresh: function () {
                    loadCaptcha();
                },
                isVerified: function () {
                    return verified;
                },
                getToken: function () {
                    return verifyToken;
                }
            };
        }
    };
})(jQuery);
