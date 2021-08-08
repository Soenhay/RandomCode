/*
Created by Soenhay

This code can be used to output information about your humble bundle games to the console. 

For the old way, go to "Keys and Entitlements". Then run the script on each page.
For the new way, go to the "Make my Choices" page for each month. Then run the script on each page.

03/06/2019 First version.
04/11/2020 Updated for Humble Choice.
8/8/2021 Updated to show monthlyTitle, claimed, gameName in csv format.

*/

//Keys and Entitlements. Pre humble choice.
if ($('.js-key-manager-holder').find('div:contains("Keys & Entitlements")').length > 0) {
  var gameNames = '';
  $('.unredeemed-keys-table').find('.game-name').each(function () {
    var monthlyTitle = $(this).find('p').attr('title');
    if(monthlyTitle && !monthlyTitle.includes('Humble Choice'))
    {
      var claimed = ($(this).siblings().find('.container > .redeemed').length > 0);
      var gameName = $(this).find('h4').attr('title');
      gameNames += `${monthlyTitle}, ${claimed}, ${gameName}\n`;
    }
  });
  console.log(gameNames);
}
else {
  //Humble choice.
  var gameNames = '';
  $('.content-choice').each(function () {
    var monthlyTitle = $('.content-choices-title > span').text().trim();
    var claimed = $(this).hasClass('claimed');
    var gameName = $(this).find('.choice-content > .title-and-delivery-methods > .content-choice-title').text().trim();
    gameNames += `${monthlyTitle}, ${claimed}, ${gameName}\n`;
  });
  console.log(gameNames);
}
