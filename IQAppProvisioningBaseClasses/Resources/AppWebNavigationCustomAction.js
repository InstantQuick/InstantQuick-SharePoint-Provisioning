(function () {
    var iqAppNavigation = {@NavigationJSON};

    if (!!iqAppNavigation && window.location.href.indexOf('IsDlg=1') === -1) {
        positionBody();
        document.addEventListener('DOMContentLoaded', injectNav);
    }

    function injectNav() {
        createLeftNav();
        createTopNav();
    }

    function createTopNav() {
        var topNavEl = document.querySelector('div.ms-breadcrumb-top') || document.querySelector('div.mp-breadcrumb-top');
        if (!topNavEl || !iqAppNavigation.TopNavigationNodes) return;

        
        var menuOuterTemplate = '<div class="ms-displayInline ms-core-navigation" id="DeltaTopNavigation" role="navigation"><div class=" noindex ms-core-listMenu-horizontalBox" id="zz11_TopNavigationMenu">{0}</div></div>';
        var menuTemplate = '<ul class="root ms-core-listMenu-root static" id="zz1_RootAspMenu">{0}</ul>';
        var menuItemTemplate = '<li class="static ms-navedit-dropNode" id="2003"><a title="{0}" class="static selected ms-navedit-dropNode menu-item ms-core-listMenu-item ms-displayInline ms-core-listMenu-selected ms-navedit-linkNode" href="{1}"><span class="additional-background ms-navedit-flyoutArrow"><span class="menu-item-text">{0}</span></span></a></li>{2}';

        var keys = Object.keys(iqAppNavigation.TopNavigationNodes);

        var menuItemsHtml = '';
        for (var i = 0; i < keys.length; i++) {
            menuItemsHtml += buildMenuItem(iqAppNavigation.TopNavigationNodes[keys[i]], menuTemplate, menuItemTemplate);
        }
        menuItemsHtml = String.format(menuOuterTemplate, String.format(menuTemplate, menuItemsHtml));
        topNavEl.innerHTML = menuItemsHtml;
    }

    function createLeftNav() {
        var leftNavEl = document.querySelector('#DeltaPlaceHolderLeftNavBar');
        if (!leftNavEl || !iqAppNavigation.LeftNavigationNodes) return;

        var menuOuterTemplate = '<div class="ms-core-sideNavBox-removeLeftMargin"><div id="ctl00_PlaceHolderLeftNavBar_QuickLaunchNavigationManager"><div class=" noindex ms-core-listMenu-verticalBox" id="zz15_V4QuickLaunchMenu">{0}</div></div></div>';
        var menuTemplate = '<ul class="root ms-core-listMenu-root static" id="zz15_RootAspMenu">{0}</ul>';
        var menuItemTemplate = '<li class="static"><a title="{0}" class="static ms-quicklaunch-dropNode menu-item ms-core-listMenu-item ms-displayInline ms-navedit-linkNode ms-droppable" aria-dropeffect="move" href="{1}"><span class="additional-background ms-navedit-flyoutArrow"><span class="menu-item-text">{0}</span></span></a>{2}</li>';

        var keys = Object.keys(iqAppNavigation.LeftNavigationNodes);

        var menuItemsHtml = '';
        for (var i = 0; i < keys.length; i++) {
            menuItemsHtml += buildMenuItem(iqAppNavigation.LeftNavigationNodes[keys[i]], menuTemplate, menuItemTemplate);
        }
        menuItemsHtml = String.format(menuOuterTemplate, String.format(menuTemplate, menuItemsHtml));
        
        leftNavEl.innerHTML = menuItemsHtml;
    }

    function buildMenuItem(navigationNode, menuTemplate, menuItemTemplate) {
        var childListHtml = '';
        if (!!navigationNode.Children && navigationNode.Children.length > 0) {
            childListHtml = buildChildListHtml(navigationNode.Children, menuItemTemplate);
        }
        var href = (!!navigationNode.Url && navigationNode.Url.indexOf('viewlsts.aspx') === -1) ? navigationNode.Url : '#';
        return String.format(menuItemTemplate, navigationNode.Title, href, childListHtml);
    }

    function buildChildListHtml(navigationNodes, menuItemTemplate) {
        var childListHtml = '';
        for (var i = 0; i < navigationNodes.length; i++) {
            var navigationNode = navigationNodes[i];
            var href = (!!navigationNode.Url && navigationNode.Url.indexOf('viewlsts.aspx') === -1) ? navigationNode.Url : '#';
            childListHtml += String.format(menuItemTemplate, navigationNode.Title, href, (!!navigationNode.Children) ? buildChildListHtml(navigationNode.Children, menuItemTemplate) : '');
        }
        return '<ul class="static">' + childListHtml + '</ul>';
    }

    function positionBody() {
        var head = document.head;
        var style = document.createElement('style');
        var rule = document.createTextNode('#contentBox {margin-left:220px}');
        style.appendChild(rule);
        head.appendChild(style);
    }
})();

