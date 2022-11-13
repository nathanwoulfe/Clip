# Clip

One of the great features of Umbraco's backoffice is the permissions model applied to content types - developers can implement a tightly controlled information architecture by configuring the parent-child relationships between those content types.

That's lovely, and allows developers plenty of control over how the site is ultimately structured.

However, there are a couple of gaps in how that model works:

- it's not possible to control who can create which types
- it's not possible to control how many of each type can be created

Fret not, Clip is here to help.

## Control who creates what

We know Umbraco offers lots of flexibility, but sometimes we need to reign that in a little.

In large sites, with lots of user groups and more content types, we need to be able to control which groups can create which types.

Clip adds this functionality to the backoffice by allowing administrators to set permitted content types for each user group.

When creating new content, the allowed child node list is filtered to include only the permitted types.

## Control how many of each type

Sometimes it doesn't make sense to allow more than one instance of a content type.

Typically, a site might have one news landing page. It more than likely has one homepage.

A site for a restaurant might have a page for each location, but shouldn't allow editors to create additional locations.

Clip adds this control to the backoffice by allowing administrators to set a maximum item count for any document type.

When creating new content, the allowed child node list is filtered to include only types that haven't hit their maximum item count.

## Why Clip?

Clip follows hot on the heels of [Flip](https://github.com/nathanwoulfe/flip) and [Blip](https://github.com/nathanwoulfe/blip), so obviously needed to follow the silly naming pattern.

Blip is the Block List Item Picker.

Flip is the document-type flipper.

Clip trims the allowed child types.

Easy!

## Getting started

Install Clip: `dotnet add package Clip.Umbraco` or `Install-Package Clip.Umbraco`.

After restarting your site, you'll find a new node in the settings tree - 'Content Creation Rules'.

The view allows adding rules for user groups and content type limits.

## Contributing

Sure, pull requests are more than welcome. Go for it.
