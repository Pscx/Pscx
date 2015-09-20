<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:maml="http://schemas.microsoft.com/maml/2004/10"
  xmlns:dev="http://schemas.microsoft.com/maml/dev/2004/10"                
  xmlns:command="http://schemas.microsoft.com/maml/dev/command/2004/10"
  >

  <xsl:output encoding="utf-8" method="xml" indent="yes" />

  <xsl:template match ="/">
    <helpItems schema="maml">
      <xsl:apply-templates />
    </helpItems>
  </xsl:template>

  <xsl:template match="p">
    <maml:para>
      <xsl:value-of select="."/>
    </maml:para>
  </xsl:template>

  <xsl:template match="/Cmdlets/Cmdlet">

    <xsl:variable name="CmdletName">
      <xsl:value-of select="Verb" />
      <xsl:text>-</xsl:text>
      <xsl:value-of select="Noun" />
    </xsl:variable>

    <xsl:variable name="Desc" select="Description"/>

    <command:command>

      <command:details>
        <command:name>
          <xsl:value-of select="$CmdletName"/>
        </command:name>

        <maml:description>
          <maml:para>
            <xsl:text>PSCX Cmdlet: </xsl:text><xsl:value-of select="normalize-space($Desc)"/>
          </maml:para>
        </maml:description>

        <maml:copyright>
          <maml:para />
        </maml:copyright>

        <command:verb>
          <xsl:value-of select="Verb" />
        </command:verb>

        <command:noun>
          <xsl:value-of select="Noun" />
        </command:noun>

        <dev:version />
      </command:details>

      <maml:description>
        <maml:para>
          <xsl:value-of select="DetailedDescription"/>
        </maml:para>
      </maml:description>

      <command:syntax>

        <xsl:for-each select="ParameterSets/ParameterSet">

          <command:syntaxItem>
            <maml:name>
              <xsl:value-of select="$CmdletName"/>
            </maml:name>

            <xsl:for-each select="Parameter">
              <xsl:sort select="@Required" order="descending"/>
              <xsl:sort select="@Position"/>
              <xsl:sort select="@Name"/>
              <xsl:call-template name="Parameter">
                <xsl:with-param name="InsideParameterSet" select="1"/>
              </xsl:call-template>
            </xsl:for-each>
          </command:syntaxItem>

        </xsl:for-each>

      </command:syntax>

      <command:parameters>
        <xsl:for-each select="Parameters/Parameter">
          <xsl:sort select="@Required" order="descending"/>
          <xsl:sort select="@Position"/>
          <xsl:sort select="@Name"/>
          <xsl:call-template name="Parameter" />
        </xsl:for-each>
      </command:parameters>

      <command:inputTypes>
        <xsl:for-each select="InputTypes/InputType">
          <command:inputType>
            <dev:type>
              <maml:name>
                <xsl:value-of select="Name"/>
              </maml:name>
              <maml:uri/>
              <maml:description>
                <xsl:apply-templates select="Description"/>
              </maml:description>
            </dev:type>
            <maml:description></maml:description>
          </command:inputType>
        </xsl:for-each>
      </command:inputTypes>

      <command:returnValues>
        <xsl:for-each select="ReturnTypes/ReturnType">
          <command:returnValue>
            <dev:type>
              <maml:name>
                <xsl:value-of select="Name"/>
              </maml:name>
              <maml:uri/>
              <maml:description>
                <xsl:apply-templates select="Description"/>
              </maml:description>
            </dev:type>
            <maml:description></maml:description>
          </command:returnValue>
        </xsl:for-each>
      </command:returnValues>

      <maml:alertSet>
        <maml:title />
        <xsl:for-each select="Notes/Note">
          <maml:alert>
            <xsl:apply-templates />
          </maml:alert>
        </xsl:for-each>
      </maml:alertSet>

      <command:examples>
        <xsl:for-each select="Examples/Example">
          <xsl:call-template name="Example"/>
        </xsl:for-each>
      </command:examples>

      <maml:relatedLinks>
        <xsl:for-each select="RelatedLinks/RelatedLink">
          <maml:navigationLink>
            <maml:linkText>
              <xsl:value-of select="."/>
            </maml:linkText>
            <maml:uri />
          </maml:navigationLink>
        </xsl:for-each>
      </maml:relatedLinks>

    </command:command>

  </xsl:template>

  <xsl:template name="Example">
    <command:example>

      <maml:title>
        <xsl:text>-------------------------- EXAMPLE </xsl:text>
        <xsl:value-of select="@Number"/>
        <xsl:text> --------------------------</xsl:text>
      </maml:title>

      <maml:introduction>
        PS C:\&gt;
      </maml:introduction>

      <dev:code>
        <xsl:value-of select="./Code"/>
      </dev:code>

      <dev:remarks>
        <xsl:apply-templates select="Remarks"/>
        <maml:para />
      </dev:remarks>

      <command:commandLines>
        <command:commandLine>
          <command:commandText />
        </command:commandLine>
      </command:commandLines>

    </command:example>
  </xsl:template>

  <xsl:template name="Parameter">
    <xsl:param name="InsideParameterSet" />

    <command:parameter>

      <xsl:attribute name="required">
        <xsl:value-of select="@Required"/>
      </xsl:attribute>

      <xsl:attribute name="globbing">
        <xsl:value-of select="@AcceptsWildcards"/>
      </xsl:attribute>

      <xsl:attribute name="pipelineInput">
        <xsl:choose>
          <xsl:when test="(@ValueFromPipeline = 'true') and (@ValueFromPipelineByPropertyName = 'true')">
            <xsl:text>true (ByValue, ByPropertyName)</xsl:text>
          </xsl:when>
          <xsl:when test="(@ValueFromPipeline = 'true')">
            <xsl:text>true (ByValue)</xsl:text>
          </xsl:when>
          <xsl:when test="(@ValueFromPipelineByPropertyName = 'true')">
            <xsl:text>true (ByPropertyName)</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>false</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>

      <xsl:attribute name="position">
        <xsl:choose>
          <xsl:when test="@Position &lt; 0">
            <xsl:text>named</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="(1 + @Position)"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>

      <maml:name>
        <xsl:value-of select="@Name"/>
      </maml:name>

      <maml:description>
        <maml:para>
          <xsl:value-of select="Description"/>
        </maml:para>
      </maml:description>

      <xsl:choose>
        <xsl:when test="$InsideParameterSet">
          <!-- In order to get simple switch paramter syntax e.g. [-Force] versus [-Force <SwitchParameter] you
               simply omit the command:parameterValue element in the syntax declaration.
          -->
          <xsl:if test="ValueType != 'SwitchParameter'">
            <command:parameterValue>
              <xsl:attribute name="required">true</xsl:attribute>
              <xsl:value-of select="ValueType"/>
            </command:parameterValue>
          </xsl:if>
        </xsl:when>

        <xsl:otherwise>
          <command:parameterValue>
            <xsl:attribute name="required">true</xsl:attribute>
            <xsl:value-of select="ValueType"/>
          </command:parameterValue>

          <dev:type>
            <maml:name>
              <xsl:value-of select="ValueType" />
            </maml:name>
            <maml:uri />
          </dev:type>

          <dev:defaultValue>
            <xsl:value-of select="DefaultValue"/>
          </dev:defaultValue>
        </xsl:otherwise>
      </xsl:choose>

    </command:parameter>
  </xsl:template>

</xsl:stylesheet>