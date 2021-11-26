.PHONY: gh-release
gh-release: ## Creates a new release by creating a new tag and pushing it
	@git stash -u
	@echo Bumping $(OLD_VERSION) to $(NEW_VERSION)…
	@sed -i 's/$(OLD_VERSION)/$(NEW_VERSION)/g' Action/*.csproj .github/workflows/*.yaml *.yaml *.md
	# @git add .
	# @git commit -s -m "Bump $(OLD_VERSION) to $(NEW_VERSION) for release"
	# @git tag -sam "$(DESCRIPTION)" $(NEW_VERSION)
	# @git push origin $(NEW_VERSION)
	# @echo
	# @echo ATTENTION: MANUAL ACTION REQUIRED!! -- Wait for the release workflow to finish
	# @echo
	# @echo Check status here https://github.com/philips-labs/slsa-provenance-action/actions/workflows/ci.yaml
	# @echo
	# @echo Once finished, push the main branch using 'git push'
	# @echo
	# @echo Visit https://github.com/philips-labs/slsa-provenance-action/releases
	# @echo Edit the release and save it to publish to GitHub Marketplace.
	# @echo
	@git stash pop