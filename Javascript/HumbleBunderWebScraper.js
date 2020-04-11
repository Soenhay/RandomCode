/*
Created by Soenhay

This code can be used to output all of your unclaimed keys to the console. 
Just click "Hide Redeemed Keys" and run it for each page of keys.
It also works on the humble choice pages. 
Once you copy all of the output into a text file you can then sort it with an online tool.

03/06/2019 First version.
04/11/2010 Updated for Humble Choice.

*/

var gameNames = '';
$('.unredeemed-keys-table').find('.game-name').each(function(){
var gameName = $(this).find('h4').attr('title'); 
  if(gameName){
  gameNames+= gameName + '\n';
  }
 });
$('.content-choice').not('.claimed').find('.choice-content > .title-and-delivery-methods > .content-choice-title').each(function(){
var choiceTitle = $('.content-choices-title > span').text().trim();
var gameName = $(this).text();
	if(gameName){
  gameNames+= choiceTitle + ': ' + gameName.trim() + '\n';
	}
});
console.log(gameNames);
