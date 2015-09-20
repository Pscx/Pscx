<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:output method="xml" />

  <xsl:template match="/">
    <Cmdlets>
      <xsl:apply-templates />
    </Cmdlets>
  </xsl:template>

  <xsl:template match="/Cmdlets/Cmdlet">

    <xsl:variable name="CmdletName">
      <xsl:value-of select="Verb" />
      <xsl:text>-</xsl:text>
      <xsl:value-of select="Noun" />
    </xsl:variable>

    <Cmdlet>
      <xsl:attribute name="Name">
        <xsl:value-of select="$CmdletName"/>
      </xsl:attribute>

      <xsl:for-each select="./Parameters/Parameter">
        <Parameter>
          <xsl:value-of select="@Name"/>
        </Parameter>
      </xsl:for-each>
    </Cmdlet>

  </xsl:template>
</xsl:stylesheet>